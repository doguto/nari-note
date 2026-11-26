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

public class BooksControllerTest : IntegrationTestBase
{
    public BooksControllerTest(NariNoteApiFactory factory) : base(factory)
    {
    }

    async Task<(HttpClient Client, CreateBookResponse Book)> CreateBookAsync(User owner, string name = "角換わり")
    {
        var client = CreateClientAs(owner);
        var response = await client.PostAsJsonAsync("/api/books", new { name });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (client, await ReadAsync<CreateBookResponse>(response));
    }

    async Task<int> PlayAsync(HttpClient client, int bookId, int positionId, params string[] usis)
    {
        var response = await client.PostAsJsonAsync($"/api/books/{bookId}/lines", new { lines = new[] { new { fromPositionId = positionId, usis } } });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await ReadAsync<SaveBookLinesResponse>(response)).Positions.Last().PositionId.Value;
    }

    [Fact]
    public async Task 作成した定跡で指した手が手順とグラフに現れる()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);
        var (client, book) = await CreateBookAsync(owner);
        var root = book.RootPositionId.Value;

        var current = await PlayAsync(client, book.Id.Value, root, "7g7f", "3c3d");
        var response = await client.GetAsync($"/api/books/{book.Id}/positions/{current}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<GetBookPositionResponse>(response);
        Assert.Equal(["▲７六歩", "△３四歩"], body.Path.Select(p => p.Label));
        Assert.True(body.IsSenteTurn);
        Assert.Equal(3, body.Graph.Nodes.Count);
        Assert.Equal(2, body.Graph.Edges.Count);
        Assert.Equal(2, body.PendingJudgeCount);

        var list = await ReadAsync<GetMyBooksResponse>(await client.GetAsync("/api/books/my"));
        Assert.Equal(3, Assert.Single(list.Books).PositionCount);
    }

    [Fact]
    public async Task 手順前後で同じ局面になると一つの局面に合流する()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);
        var (client, book) = await CreateBookAsync(owner);
        var root = book.RootPositionId.Value;

        var a = await PlayAsync(client, book.Id.Value, root, "7g7f", "3c3d", "2g2f");
        var b = await PlayAsync(client, book.Id.Value, root, "2g2f", "3c3d", "7g7f");

        Assert.Equal(a, b);
        var count = await QueryAsync(db => db.BookPositions.CountAsync(p => p.BookId == book.Id));
        Assert.Equal(6, count);
    }

    [Fact]
    public async Task 指せない手は400を返す()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);
        var (client, book) = await CreateBookAsync(owner);

        var response = await client.PostAsJsonAsync($"/api/books/{book.Id}/lines", new { lines = new[] { new { fromPositionId = book.RootPositionId.Value, usis = new[] { "7g7f", "7g7e" } } } });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task 手を外すと先の局面も消える()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);
        var (client, book) = await CreateBookAsync(owner);
        var root = book.RootPositionId.Value;
        await PlayAsync(client, book.Id.Value, root, "7g7f", "3c3d");
        var moveId = await QueryAsync(db => db.BookMoves.Where(m => m.Usi == "7g7f").Select(m => m.Id).SingleAsync());

        var response = await client.DeleteAsync($"/api/books/{book.Id}/moves/{moveId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(1, await QueryAsync(db => db.BookPositions.CountAsync()));
        Assert.Equal(0, await QueryAsync(db => db.BookMoves.CountAsync()));
        Assert.Equal(1, await QueryAsync(db => db.ShapeGroups.CountAsync()));
    }

    [Fact]
    public async Task 形の名前と研究メモと手のメモを保存できる()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);
        var (client, book) = await CreateBookAsync(owner);
        var child = await PlayAsync(client, book.Id.Value, book.RootPositionId.Value, "7g7f");
        var state = await ReadAsync<GetBookPositionResponse>(await client.GetAsync($"/api/books/{book.Id}/positions/{child}"));
        var moveId = await QueryAsync(db => db.BookMoves.Select(m => m.Id).SingleAsync());

        var groupResponse = await client.PutAsJsonAsync($"/api/books/{book.Id}/groups/{state.Group.Id}", new { name = "角道を開けた形", memo = "次は△３四歩" });
        var moveResponse = await client.PutAsJsonAsync($"/api/books/{book.Id}/moves/{moveId}", new { memo = "角道を開ける" });

        Assert.Equal(HttpStatusCode.OK, groupResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, moveResponse.StatusCode);
        var after = await ReadAsync<GetBookPositionResponse>(await client.GetAsync($"/api/books/{book.Id}/positions/{book.RootPositionId}"));
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
        var (client, book) = await CreateBookAsync(owner);
        Factory.ShogiEngineGateway.Lines.Add(new EngineLine { Usi = "7g7f", Score = 40, Pv = ["7g7f", "3c3d"] });

        var response = await client.PostAsJsonAsync(
            $"/api/books/{book.Id}/analysis",
            new { sfen = ShogiPosition.Hirate, engine = AnalysisEngine.TsuyouraouMicro }
        );

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await ReadAsync<AnalyzeBookPositionResponse>(response);
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
        var (client, book) = await CreateBookAsync(owner);

        var response = await client.PostAsJsonAsync($"/api/books/{book.Id}/analysis", new { sfen = ShogiPosition.Hirate, engine = "unknown" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(Factory.ShogiEngineGateway.AnalyzedEngines);
    }

    [Fact]
    public async Task 未ログインでは401を返す()
    {
        var response = await CreateClient().GetAsync("/api/books/my");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task 他人の定跡は見ることも変えることもできない()
    {
        var owner = new UserBuilder().Build();
        var other = new UserBuilder().Build();
        await SeedAsync(owner, other);
        var (_, book) = await CreateBookAsync(owner);
        var client = CreateClientAs(other);

        var get = await client.GetAsync($"/api/books/{book.Id}/positions/{book.RootPositionId}");
        var move = await client.PostAsJsonAsync($"/api/books/{book.Id}/lines", new { lines = new[] { new { fromPositionId = book.RootPositionId.Value, usis = new[] { "7g7f" } } } });
        var delete = await client.DeleteAsync($"/api/books/{book.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, get.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, move.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, delete.StatusCode);
        var othersList = await ReadAsync<GetMyBooksResponse>(await client.GetAsync("/api/books/my"));
        Assert.Empty(othersList.Books);
    }

    [Fact]
    public async Task 定跡を削除すると局面と手もすべて消える()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);
        var (client, book) = await CreateBookAsync(owner);
        await PlayAsync(client, book.Id.Value, book.RootPositionId.Value, "7g7f", "3c3d");

        var response = await client.DeleteAsync($"/api/books/{book.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(0, await QueryAsync(db => db.Books.CountAsync()));
        Assert.Equal(0, await QueryAsync(db => db.BookPositions.CountAsync()));
        Assert.Equal(0, await QueryAsync(db => db.ShapeGroups.CountAsync()));
    }

    [Fact]
    public async Task 名前が空の定跡は作れない()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);

        var response = await CreateClientAs(owner).PostAsJsonAsync("/api/books", new { name = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
