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
    public async Task 保存した手順は1手ごとに局面と遷移になる()
    {
        var (client, id) = await CreatePlaybookAsync();

        await SaveAsync(client, id, ["7g7f", "3c3d", "2g2f"]);

        var playbook = await GetAsync(client, id);
        Assert.Equal(4, playbook.Boards.Count);
        Assert.Contains(playbook.Boards, b => b.Sfen == Root);
        Assert.Equal(["7g7f", "3c3d", "2g2f"], playbook.Transitions.Select(t => t.Usi));
        Assert.Empty(playbook.Nodes);
    }

    [Fact]
    public async Task 手順を足しても既存の遷移の条件は残る()
    {
        var (client, id) = await CreatePlaybookAsync();
        await SaveAsync(client, id, ["7g7f", "3c3d", "2g2f"]);
        var branch = PlaybookBuilder.Play(Root, "7g7f", "3c3d");
        var before = await GetAsync(client, id);
        var transitionId = before.Transitions.Single(t => t.FromBoardId == BoardIdOf(before, branch)).Id;
        var update = await client.PutAsJsonAsync($"/api/playbooks/{id}/transitions/{transitionId}", new { condition = "角道を開けた" });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        await SaveAsync(client, id, ["7g7f", "3c3d", "6g6f"]);

        var playbook = await GetAsync(client, id);
        Assert.Equal(5, playbook.Boards.Count);
        Assert.Equal(2, playbook.Transitions.Count(t => t.FromBoardId == BoardIdOf(playbook, branch)));
        Assert.Equal("角道を開けた", playbook.Transitions.Single(t => t.Id == transitionId).Condition);
    }

    [Fact]
    public async Task 手順前後で同じ局面になると一つの局面に合流する()
    {
        var (client, id) = await CreatePlaybookAsync();

        await SaveAsync(client, id, ["7g7f", "3c3d", "2g2f", "8c8d"], ["2g2f", "3c3d", "7g7f"]);

        var playbook = await GetAsync(client, id);
        var merged = PlaybookBuilder.Play(Root, "7g7f", "3c3d", "2g2f");
        var board = playbook.Boards.Single(b => b.Sfen == merged);
        Assert.Equal(2, playbook.Transitions.Count(t => t.ToBoardId == board.Id));
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
    public async Task 手を外すとその先にしかない局面とノードも消える()
    {
        var (client, id) = await CreatePlaybookAsync();
        await SaveAsync(client, id, ["7g7f", "3c3d", "2g2f"], ["7g7f", "3c3d", "6g6f"]);
        var branch = PlaybookBuilder.Play(Root, "7g7f", "3c3d");
        await client.PutAsJsonAsync($"/api/playbooks/{id}/nodes", new { sfen = PlaybookBuilder.Play(branch, "6g6f"), name = "四間飛車" });

        var response = await client.PostAsJsonAsync($"/api/playbooks/{id}/moves/remove", new { sfen = branch, usi = "6g6f" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var playbook = await GetAsync(client, id);
        Assert.Equal(4, playbook.Boards.Count);
        Assert.Equal(["7g7f", "3c3d", "2g2f"], playbook.Transitions.Select(t => t.Usi));
        Assert.Empty(playbook.Nodes);
    }

    [Fact]
    public async Task 局面に名前を付けるとノードになり名前とメモを消すとノードから外れる()
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

        await client.PutAsJsonAsync($"/api/playbooks/{id}/nodes", new { sfen = middle, name = "", memo = "" });

        var cleared = await GetAsync(client, id);
        Assert.Empty(cleared.Nodes);
        Assert.Null(cleared.Boards.Single(b => b.Sfen == middle).NodeId);
        Assert.Equal(4, cleared.Boards.Count);
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
