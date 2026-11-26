using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Shogi;
using NariNoteBackend.Tests.Support.Builder;
using NariNoteBackend.Tests.Support.Integration;

namespace NariNoteBackend.Tests.Controller;

public class PlaybooksControllerTest : IntegrationTestBase
{
    public PlaybooksControllerTest(NariNoteApiFactory factory) : base(factory)
    {
    }

    async Task<(HttpClient Client, CreatePlaybookResponse Playbook)> CreatePlaybookAsync(User owner, string name = "角換わり")
    {
        var client = CreateClientAs(owner);
        var response = await client.PostAsJsonAsync("/api/playbooks", new { name });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (client, await ReadAsync<CreatePlaybookResponse>(response));
    }

    async Task<Guid> PlayAsync(HttpClient client, Guid playbookId, Guid positionId, params string[] usis)
    {
        var response = await client.PostAsJsonAsync($"/api/playbooks/{playbookId}/lines", new { lines = new[] { new { fromPositionId = positionId, usis } } });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await ReadAsync<SavePlaybookLinesResponse>(response)).Positions.Last().PositionId.Value;
    }

    [Fact]
    public async Task 作成した定跡で指した手が手順とグラフに現れる()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);
        var (client, playbook) = await CreatePlaybookAsync(owner);
        var root = playbook.RootPositionId.Value;

        var current = await PlayAsync(client, playbook.Id.Value, root, "7g7f", "3c3d");
        var response = await client.GetAsync($"/api/playbooks/{playbook.Id}/positions/{current}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<GetPlaybookPositionResponse>(response);
        Assert.Equal(["▲７六歩", "△３四歩"], body.Path.Select(p => p.Label));
        Assert.True(body.IsSenteTurn);
        Assert.Equal(3, body.Graph.Nodes.Count);
        Assert.Equal(2, body.Graph.Edges.Count);
        Assert.Equal(2, body.PendingJudgeCount);

        var list = await ReadAsync<GetMyPlaybooksResponse>(await client.GetAsync("/api/playbooks/my"));
        Assert.Equal(3, Assert.Single(list.Playbooks).PositionCount);
    }

    [Fact]
    public async Task 手順前後で同じ局面になると一つの局面に合流する()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);
        var (client, playbook) = await CreatePlaybookAsync(owner);
        var root = playbook.RootPositionId.Value;

        var a = await PlayAsync(client, playbook.Id.Value, root, "7g7f", "3c3d", "2g2f");
        var b = await PlayAsync(client, playbook.Id.Value, root, "2g2f", "3c3d", "7g7f");

        Assert.Equal(a, b);
        var count = await QueryAsync(db => db.PlaybookPositions.CountAsync(p => p.PlaybookId == playbook.Id));
        Assert.Equal(6, count);
    }

    [Fact]
    public async Task 指せない手は400を返す()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);
        var (client, playbook) = await CreatePlaybookAsync(owner);

        var response = await client.PostAsJsonAsync($"/api/playbooks/{playbook.Id}/lines", new { lines = new[] { new { fromPositionId = playbook.RootPositionId.Value, usis = new[] { "7g7f", "7g7e" } } } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task 手を外すと先の局面も消える()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);
        var (client, playbook) = await CreatePlaybookAsync(owner);
        var root = playbook.RootPositionId.Value;
        await PlayAsync(client, playbook.Id.Value, root, "7g7f", "3c3d");
        var moveId = await QueryAsync(db => db.PlaybookMoves.Where(m => m.Usi == "7g7f").Select(m => m.Id).SingleAsync());

        var response = await client.DeleteAsync($"/api/playbooks/{playbook.Id}/moves/{moveId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(1, await QueryAsync(db => db.PlaybookPositions.CountAsync()));
        Assert.Equal(0, await QueryAsync(db => db.PlaybookMoves.CountAsync()));
        Assert.Equal(1, await QueryAsync(db => db.ShapeGroups.CountAsync()));
    }

    [Fact]
    public async Task 形の名前と研究メモと手のメモを保存できる()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);
        var (client, playbook) = await CreatePlaybookAsync(owner);
        var child = await PlayAsync(client, playbook.Id.Value, playbook.RootPositionId.Value, "7g7f");
        var state = await ReadAsync<GetPlaybookPositionResponse>(await client.GetAsync($"/api/playbooks/{playbook.Id}/positions/{child}"));
        var moveId = await QueryAsync(db => db.PlaybookMoves.Select(m => m.Id).SingleAsync());

        var groupResponse = await client.PutAsJsonAsync($"/api/playbooks/{playbook.Id}/groups/{state.Group.Id}", new { name = "角道を開けた形", memo = "次は△３四歩" });
        var moveResponse = await client.PutAsJsonAsync($"/api/playbooks/{playbook.Id}/moves/{moveId}", new { memo = "角道を開ける" });

        Assert.Equal(HttpStatusCode.OK, groupResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, moveResponse.StatusCode);
        var after = await ReadAsync<GetPlaybookPositionResponse>(await client.GetAsync($"/api/playbooks/{playbook.Id}/positions/{playbook.RootPositionId}"));
        Assert.Equal("角道を開ける", Assert.Single(after.Moves).Memo);
        var group = await QueryAsync(db => db.ShapeGroups.SingleAsync(g => g.Id == state.Group.Id));
        Assert.Equal("角道を開けた形", group.Name);
        Assert.Equal("次は△３四歩", group.Memo);
    }

    [Fact]
    public async Task 解析はエンジンの読み筋を記譜で返す()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);
        var (client, playbook) = await CreatePlaybookAsync(owner);
        Factory.ShogiEngineGateway.Lines.Add(new EngineLine { Usi = "7g7f", Score = 40, Pv = ["7g7f", "3c3d"] });

        var response = await client.PostAsJsonAsync(
            $"/api/playbooks/{playbook.Id}/analysis",
            new { sfen = ShogiPosition.Hirate, engine = AnalysisEngine.TsuyouraouMicro }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<AnalyzePlaybookPositionResponse>(response);
        var line = Assert.Single(body.Lines);
        Assert.Equal(["▲７六歩", "△３四歩"], line.Pv);
        Assert.Equal(40, line.Score);
        Assert.Equal([AnalysisEngine.TsuyouraouMicro], Factory.ShogiEngineGateway.AnalyzedEngines);
    }

    [Fact]
    public async Task 未対応のエンジンでの解析は400を返す()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);
        var (client, playbook) = await CreatePlaybookAsync(owner);

        var response = await client.PostAsJsonAsync($"/api/playbooks/{playbook.Id}/analysis", new { sfen = ShogiPosition.Hirate, engine = "unknown" });

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
        var (_, playbook) = await CreatePlaybookAsync(owner);
        var client = CreateClientAs(other);

        var get = await client.GetAsync($"/api/playbooks/{playbook.Id}/positions/{playbook.RootPositionId}");
        var move = await client.PostAsJsonAsync($"/api/playbooks/{playbook.Id}/lines", new { lines = new[] { new { fromPositionId = playbook.RootPositionId.Value, usis = new[] { "7g7f" } } } });
        var delete = await client.DeleteAsync($"/api/playbooks/{playbook.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, get.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, move.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
        var othersList = await ReadAsync<GetMyPlaybooksResponse>(await client.GetAsync("/api/playbooks/my"));
        Assert.Empty(othersList.Playbooks);
    }

    [Fact]
    public async Task 定跡を削除すると局面と手もすべて消える()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);
        var (client, playbook) = await CreatePlaybookAsync(owner);
        await PlayAsync(client, playbook.Id.Value, playbook.RootPositionId.Value, "7g7f", "3c3d");

        var response = await client.DeleteAsync($"/api/playbooks/{playbook.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(0, await QueryAsync(db => db.Playbooks.CountAsync()));
        Assert.Equal(0, await QueryAsync(db => db.PlaybookPositions.CountAsync()));
        Assert.Equal(0, await QueryAsync(db => db.ShapeGroups.CountAsync()));
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
