using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Shogi;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Tests.Support.Builder;
using NariNoteBackend.Tests.Support.Integration;

namespace NariNoteBackend.Tests.Controller;

public class PlaybooksControllerTest : IntegrationTestBase
{
    static readonly string Root = ShogiBoard.Normalize(ShogiBoard.Hirate);

    public PlaybooksControllerTest(NariNoteApiFactory factory) : base(factory)
    {
    }

    async Task<(HttpClient Client, Guid PlaybookId)> CreatePlaybookAsync(User owner, string name = "角換わり")
    {
        var client = CreateClientAs(owner);
        var response = await client.PostAsJsonAsync("/api/playbooks", new { name });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (client, (await ReadAsync<CreatePlaybookResponse>(response)).Id.Value);
    }

    async Task<(HttpClient Client, Guid PlaybookId)> CreatePlaybookAsync()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);
        return await CreatePlaybookAsync(owner);
    }

    static async Task SaveAsync(HttpClient client, Guid playbookId, params string[][] lines)
    {
        var response = await client.PostAsJsonAsync($"/api/playbooks/{playbookId}/lines", new { lines = lines.Select(usis => new { usis }) });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    async Task<GetPlaybookResponse> GetAsync(HttpClient client, Guid playbookId)
    {
        var response = await client.GetAsync($"/api/playbooks/{playbookId}");
        Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync());
        return await ReadAsync<GetPlaybookResponse>(response);
    }

    static PlaybookBoardId BoardIdOf(GetPlaybookResponse playbook, string sfen) => playbook.Boards.Single(b => b.Sfen == sfen).Id;

    [Fact]
    public async Task 保存した手順は開始局面から終点までの1つの手順になる()
    {
        var (client, id) = await CreatePlaybookAsync();

        await SaveAsync(client, id, ["7g7f", "3c3d", "2g2f"]);

        var playbook = await GetAsync(client, id);
        var root = Assert.Single(playbook.Boards);
        Assert.Equal(Root, root.Sfen);
        var transition = Assert.Single(playbook.Transitions);
        Assert.Equal(["7g7f", "3c3d", "2g2f"], transition.Usis);
        Assert.Null(transition.ToBoardId);
    }

    [Fact]
    public async Task 手順の途中で分かれると分岐点がノードになり既存の手順の条件は残る()
    {
        var (client, id) = await CreatePlaybookAsync();
        await SaveAsync(client, id, ["7g7f", "3c3d", "2g2f"]);
        var transitionId = Assert.Single((await GetAsync(client, id)).Transitions).Id;
        var update = await client.PutAsJsonAsync($"/api/playbooks/{id}/transitions/{transitionId}", new { condition = "角道を開けた" });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        await SaveAsync(client, id, ["7g7f", "3c3d", "6g6f"]);

        var playbook = await GetAsync(client, id);
        var branch = PlaybookBuilder.Play(Root, "7g7f", "3c3d");
        Assert.Equal(2, playbook.Boards.Count);
        Assert.Equal(3, playbook.Transitions.Count);
        var first = playbook.Transitions.Single(t => t.Id == transitionId);
        Assert.Equal(["7g7f", "3c3d"], first.Usis);
        Assert.Equal("角道を開けた", first.Condition);
        Assert.Equal(BoardIdOf(playbook, branch), first.ToBoardId);
    }

    [Fact]
    public async Task 手順前後で同じ局面になると一つのノードに合流する()
    {
        var (client, id) = await CreatePlaybookAsync();

        await SaveAsync(client, id, ["7g7f", "3c3d", "2g2f", "8c8d"], ["2g2f", "3c3d", "7g7f"]);

        var playbook = await GetAsync(client, id);
        var merged = PlaybookBuilder.Play(Root, "7g7f", "3c3d", "2g2f");
        var board = playbook.Boards.Single(b => b.Sfen == merged);
        Assert.Equal(2, playbook.Transitions.Count(s => s.ToBoardId == board.Id));
        Assert.Equal(1, await QueryAsync(db => db.PlaybookBoards.CountAsync(b => b.Sfen == merged)));
    }

    [Fact]
    public async Task 指せない手が含まれると何も保存しない()
    {
        var (client, id) = await CreatePlaybookAsync();

        var response = await client.PostAsJsonAsync($"/api/playbooks/{id}/lines", new { lines = new[] { new { usis = new[] { "7g7f", "7g7e" } } } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0, await QueryAsync(db => db.PlaybookBoardTransitions.CountAsync()));
    }

    [Fact]
    public async Task 辺を消すとその先にしか無いノードも消え分岐点のノードは残る()
    {
        var (client, id) = await CreatePlaybookAsync();
        await SaveAsync(client, id, ["7g7f", "3c3d", "2g2f", "8c8d"], ["7g7f", "3c3d", "6g6f"]);
        var branch = PlaybookBuilder.Play(Root, "7g7f", "3c3d");
        await client.PutAsJsonAsync($"/api/playbooks/{id}/nodes", new { sfen = PlaybookBuilder.Play(branch, "2g2f"), name = "居飛車" });
        var before = await GetAsync(client, id);
        var toIbisha = before.Transitions.Single(t => t.FromBoardId == BoardIdOf(before, branch) && t.Usis[0] == "2g2f");

        var response = await client.DeleteAsync($"/api/playbooks/{id}/transitions/{toIbisha.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var playbook = await GetAsync(client, id);
        Assert.DoesNotContain(playbook.Nodes, n => n.Name == "居飛車");
        Assert.Equal(new[] { Root, branch }.Order(), playbook.Boards.Select(b => b.Sfen).Order());
        Assert.Equal(["6g6f"], playbook.Transitions.Single(t => t.FromBoardId == BoardIdOf(playbook, branch)).Usis);
    }

    [Fact]
    public async Task 名前を付けずに辺の途中の局面をノードにすると辺が分かれ条件は出発側に残る()
    {
        var (client, id) = await CreatePlaybookAsync();
        await SaveAsync(client, id, ["7g7f", "3c3d", "2g2f"]);
        var transitionId = Assert.Single((await GetAsync(client, id)).Transitions).Id;
        await client.PutAsJsonAsync($"/api/playbooks/{id}/transitions/{transitionId}", new { condition = "角道を開けた" });
        var middle = PlaybookBuilder.Play(Root, "7g7f");

        var response = await client.PutAsJsonAsync($"/api/playbooks/{id}/nodes", new { sfen = middle });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var playbook = await GetAsync(client, id);
        var board = playbook.Boards.Single(b => b.Sfen == middle);
        Assert.Equal("", playbook.Nodes.Single(n => n.Id == board.NodeId).Name);
        var first = playbook.Transitions.Single(t => t.Id == transitionId);
        Assert.Equal(["7g7f"], first.Usis);
        Assert.Equal(board.Id, first.ToBoardId);
        Assert.Equal("角道を開けた", first.Condition);
        var second = playbook.Transitions.Single(t => t.FromBoardId == board.Id);
        Assert.Equal(["3c3d", "2g2f"], second.Usis);
    }

    [Fact]
    public async Task ノードを解除すると前後の辺が一本につながる()
    {
        var (client, id) = await CreatePlaybookAsync();
        await SaveAsync(client, id, ["7g7f", "3c3d", "2g2f"]);
        var middle = PlaybookBuilder.Play(Root, "7g7f");
        await client.PutAsJsonAsync($"/api/playbooks/{id}/nodes", new { sfen = middle, name = "角道を開けた形" });
        var before = await GetAsync(client, id);
        var nodeId = before.Boards.Single(b => b.Sfen == middle).NodeId;

        var response = await client.PostAsync($"/api/playbooks/{id}/nodes/{nodeId}/unlink", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var playbook = await GetAsync(client, id);
        Assert.Single(playbook.Boards);
        Assert.Equal(["7g7f", "3c3d", "2g2f"], Assert.Single(playbook.Transitions).Usis);
    }

    [Fact]
    public async Task 分岐点のノードは解除できない()
    {
        var (client, id) = await CreatePlaybookAsync();
        await SaveAsync(client, id, ["7g7f", "3c3d", "2g2f"], ["7g7f", "3c3d", "6g6f"]);
        var playbook = await GetAsync(client, id);
        var nodeId = playbook.Boards.Single(b => b.Sfen == PlaybookBuilder.Play(Root, "7g7f", "3c3d")).NodeId;

        var response = await client.PostAsync($"/api/playbooks/{id}/nodes/{nodeId}/unlink", null);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ノードを削除するとその先にしか無い辺とノードも消える()
    {
        var (client, id) = await CreatePlaybookAsync();
        await SaveAsync(client, id, ["7g7f", "3c3d", "2g2f", "8c8d"], ["2g2f", "8c8d"]);
        var x = PlaybookBuilder.Play(Root, "7g7f", "3c3d");
        await client.PutAsJsonAsync($"/api/playbooks/{id}/nodes", new { sfen = x, name = "角道を開け合った形" });
        var before = await GetAsync(client, id);
        var nodeId = before.Boards.Single(b => b.Sfen == x).NodeId;

        var response = await client.DeleteAsync($"/api/playbooks/{id}/nodes/{nodeId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var playbook = await GetAsync(client, id);
        Assert.Single(playbook.Boards);
        Assert.Equal(["2g2f", "8c8d"], Assert.Single(playbook.Transitions).Usis);
    }

    [Fact]
    public async Task 開始局面のノードは削除も解除もできない()
    {
        var (client, id) = await CreatePlaybookAsync();
        var nodeId = Assert.Single((await GetAsync(client, id)).Nodes).Id;

        var delete = await client.DeleteAsync($"/api/playbooks/{id}/nodes/{nodeId}");
        var unlink = await client.PostAsync($"/api/playbooks/{id}/nodes/{nodeId}/unlink", null);

        Assert.Equal(HttpStatusCode.BadRequest, delete.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, unlink.StatusCode);
    }

    [Fact]
    public async Task 辺の途中の局面に名前を付けるとノードになり名前とメモを消すと戻る()
    {
        var (client, id) = await CreatePlaybookAsync();
        await SaveAsync(client, id, ["7g7f", "3c3d", "2g2f"]);
        var middle = PlaybookBuilder.Play(Root, "7g7f", "3c3d");

        var named = await client.PutAsJsonAsync($"/api/playbooks/{id}/nodes", new { sfen = middle, name = "角道を開け合った形", memo = "次は飛車先" });

        Assert.Equal(HttpStatusCode.OK, named.StatusCode);
        var playbook = await GetAsync(client, id);
        var board = playbook.Boards.Single(b => b.Sfen == middle);
        var node = playbook.Nodes.Single(n => n.Id == board.NodeId);
        Assert.Equal("角道を開け合った形", node.Name);
        Assert.Equal("次は飛車先", node.Memo);
        Assert.Equal(2, playbook.Transitions.Count);

        await client.PutAsJsonAsync($"/api/playbooks/{id}/nodes", new { sfen = middle, name = "", memo = "" });

        var cleared = await GetAsync(client, id);
        Assert.Single(cleared.Boards);
        Assert.Single(cleared.Transitions);
    }

    [Fact]
    public async Task 似た局面をまとめると一つのノードになり分けると戻る()
    {
        var (client, id) = await CreatePlaybookAsync();
        var a = PlaybookBuilder.Play(Root, "7g7f", "3c3d", "2g2f");
        var b = PlaybookBuilder.Play(Root, "9g9f", "9c9d", "7g7f", "3c3d", "2g2f");
        await SaveAsync(client, id, ["7g7f", "3c3d", "2g2f"], ["9g9f", "9c9d", "7g7f", "3c3d", "2g2f"]);
        await client.PutAsJsonAsync($"/api/playbooks/{id}/nodes", new { sfen = a, name = "角道を開けた形" });
        await client.PutAsJsonAsync($"/api/playbooks/{id}/nodes", new { sfen = b, memo = "端歩の研究" });
        var before = await GetAsync(client, id);

        var inspection = await ReadAsync<InspectPlaybookBoardResponse>(await client.GetAsync($"/api/playbooks/{id}/inspection?sfen={Uri.EscapeDataString(b)}"));
        var similar = Assert.Single(inspection.Similar);
        Assert.Equal(BoardIdOf(before, a), similar.BoardId);

        var merge = await client.PostAsJsonAsync($"/api/playbooks/{id}/nodes/merge", new { boardId = BoardIdOf(before, a).Value, otherBoardId = BoardIdOf(before, b).Value });

        Assert.Equal(HttpStatusCode.OK, merge.StatusCode);
        var merged = await GetAsync(client, id);
        var node = Assert.Single(merged.Nodes, n => n.Name == "角道を開けた形");
        Assert.Equal("端歩の研究", node.Memo);
        Assert.Equal(2, merged.Boards.Count(x => x.NodeId == node.Id));

        var split = await client.PostAsync($"/api/playbooks/{id}/boards/{BoardIdOf(merged, b)}/split", null);

        Assert.Equal(HttpStatusCode.OK, split.StatusCode);
        var after = await GetAsync(client, id);
        Assert.Single(after.Boards, x => x.NodeId == node.Id);
    }

    [Fact]
    public async Task 定跡に無い局面は調べられない()
    {
        var (client, id) = await CreatePlaybookAsync();

        var response = await client.GetAsync($"/api/playbooks/{id}/inspection?sfen={Uri.EscapeDataString(PlaybookBuilder.Play(Root, "2g2f"))}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task 解析はエンジンの読み筋を記譜で返す()
    {
        var (client, id) = await CreatePlaybookAsync();
        Factory.ShogiEngineGateway.Lines.Add(new EngineLine { Usi = "7g7f", Score = 40, Pv = ["7g7f", "3c3d"] });

        var response = await client.PostAsJsonAsync($"/api/playbooks/{id}/analysis", new { sfen = ShogiBoard.Hirate, engine = AnalysisEngine.TsuyouraouMicro });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<AnalyzePlaybookBoardResponse>(response);
        var line = Assert.Single(body.Lines);
        Assert.Equal(["▲７六歩", "△３四歩"], line.Pv);
        Assert.Equal(40, line.Score);
        Assert.Equal([AnalysisEngine.TsuyouraouMicro], Factory.ShogiEngineGateway.AnalyzedEngines);
    }

    [Fact]
    public async Task 未対応のエンジンでの解析は400を返す()
    {
        var (client, id) = await CreatePlaybookAsync();

        var response = await client.PostAsJsonAsync($"/api/playbooks/{id}/analysis", new { sfen = ShogiBoard.Hirate, engine = "unknown" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(Factory.ShogiEngineGateway.AnalyzedEngines);
    }

    [Fact]
    public async Task 未ログインでは401を返す()
    {
        var response = await CreateClient().GetAsync("/api/playbooks/my");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task 他人の定跡は見ることも変えることもできない()
    {
        var owner = new UserBuilder().Build();
        var other = new UserBuilder().Build();
        await SeedAsync(owner, other);
        var (_, id) = await CreatePlaybookAsync(owner);
        var client = CreateClientAs(other);

        var get = await client.GetAsync($"/api/playbooks/{id}");
        var save = await client.PostAsJsonAsync($"/api/playbooks/{id}/lines", new { lines = new[] { new { usis = new[] { "7g7f" } } } });
        var delete = await client.DeleteAsync($"/api/playbooks/{id}");

        Assert.Equal(HttpStatusCode.Forbidden, get.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, save.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
        var othersList = await ReadAsync<GetMyPlaybooksResponse>(await client.GetAsync("/api/playbooks/my"));
        Assert.Empty(othersList.Playbooks);
    }

    [Fact]
    public async Task 定跡を削除するとノードと局面と遷移もすべて消える()
    {
        var (client, id) = await CreatePlaybookAsync();
        await SaveAsync(client, id, ["7g7f", "3c3d", "2g2f"], ["7g7f", "3c3d", "6g6f"]);

        var response = await client.DeleteAsync($"/api/playbooks/{id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(0, await QueryAsync(db => db.Playbooks.CountAsync()));
        Assert.Equal(0, await QueryAsync(db => db.PlaybookNodes.CountAsync()));
        Assert.Equal(0, await QueryAsync(db => db.PlaybookBoards.CountAsync()));
        Assert.Equal(0, await QueryAsync(db => db.PlaybookBoardTransitions.CountAsync()));
    }

    [Fact]
    public async Task 名前が空の定跡は作れない()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);

        var response = await CreateClientAs(owner).PostAsJsonAsync("/api/playbooks", new { name = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
