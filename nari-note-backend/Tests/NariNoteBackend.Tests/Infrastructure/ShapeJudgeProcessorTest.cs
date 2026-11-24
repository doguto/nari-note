using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Infrastructure.Job;
using NariNoteBackend.Tests.Support.Builder;
using NariNoteBackend.Tests.Support.Integration;

namespace NariNoteBackend.Tests.Infrastructure;

public class ShapeJudgeProcessorTest : IntegrationTestBase
{
    public ShapeJudgeProcessorTest(NariNoteApiFactory factory) : base(factory)
    {
    }

    async Task ProcessAllAsync()
    {
        for (var i = 0; i < 50; i++)
        {
            using var scope = Factory.Services.CreateScope();
            if (!await scope.ServiceProvider.GetRequiredService<ShapeJudgeProcessor>().ProcessAsync(CancellationToken.None)) return;
        }
        throw new InvalidOperationException("判定が終わらない");
    }

    async Task<(HttpClient Client, int BookId, int A, int B)> CreateTwoLinesAsync()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);
        var client = CreateClientAs(owner);
        var book = await ReadAsync<CreateBookResponse>(await client.PostAsJsonAsync("/api/books", new { name = "角換わり" }));
        var bookId = book.Id.Value;

        async Task<int> Play(int from, params string[] usis)
        {
            var res = await client.PostAsJsonAsync($"/api/books/{bookId}/lines", new { lines = new[] { new { fromPositionId = from, usis } } });
            return (await ReadAsync<SaveBookLinesResponse>(res)).Positions.Last().PositionId.Value;
        }

        var a = await Play(book.RootPositionId.Value, "7g7f", "3c3d");
        var b = await Play(book.RootPositionId.Value, "9g9f", "9c9d", "7g7f", "3c3d");
        return (client, bookId, a, b);
    }

    [Fact]
    public async Task 新しい局面ごとに候補を探して判定し同じなら提案になる()
    {
        var (client, bookId, a, b) = await CreateTwoLinesAsync();
        Factory.ShogiEngineGateway.Candidates = null;
        Factory.ShogiEngineGateway.Verdict = new EngineVerdict { Kind = ShapeCandidate.VerdictSameWithDelta, Delta = 35, ConfirmedDepth = 6 };

        await ProcessAllAsync();

        Assert.Equal(0, await QueryAsync(db => db.BookPositions.CountAsync(p => !p.CandidatesScanned)));
        Assert.All(await QueryAsync(db => db.ShapeCandidates.ToListAsync()), c => Assert.Equal(ShapeCandidateStatus.Done, c.Status));
        var state = await ReadAsync<GetBookPositionResponse>(await client.GetAsync($"/api/books/{bookId}/positions/{b}"));
        Assert.Contains(state.Proposals, p => p.OtherPositionId.Value == a && p.Verdict == ShapeCandidate.VerdictSameWithDelta);
        Assert.Equal(0, state.PendingJudgeCount);
    }

    [Fact]
    public async Task 提案をまとめると同じ形のノードになり分けると戻る()
    {
        var (client, bookId, a, b) = await CreateTwoLinesAsync();
        await ProcessAllAsync();
        var state = await ReadAsync<GetBookPositionResponse>(await client.GetAsync($"/api/books/{bookId}/positions/{b}"));
        var proposal = state.Proposals.First(p => p.OtherPositionId.Value == a);

        var merge = await client.PostAsync($"/api/books/{bookId}/candidates/{proposal.CandidateId}/merge", null);

        Assert.Equal(HttpStatusCode.OK, merge.StatusCode);
        var merged = await ReadAsync<GetBookPositionResponse>(await client.GetAsync($"/api/books/{bookId}/positions/{a}"));
        Assert.Equal(2, merged.Group.Members.Count);
        Assert.Equal(2, merged.Graph.Nodes.Single(n => n.GroupId == merged.Group.Id).PositionCount);

        var split = await client.PostAsync($"/api/books/{bookId}/positions/{b}/split", null);

        Assert.Equal(HttpStatusCode.OK, split.StatusCode);
        var after = await ReadAsync<GetBookPositionResponse>(await client.GetAsync($"/api/books/{bookId}/positions/{a}"));
        Assert.Single(after.Group.Members);
    }

    [Fact]
    public async Task 別と判定された候補は提案にならない()
    {
        var (client, bookId, _, b) = await CreateTwoLinesAsync();
        Factory.ShogiEngineGateway.Verdict = new EngineVerdict { Kind = ShapeCandidate.VerdictDifferent, ConfirmedDepth = 0 };

        await ProcessAllAsync();

        var state = await ReadAsync<GetBookPositionResponse>(await client.GetAsync($"/api/books/{bookId}/positions/{b}"));
        Assert.Empty(state.Proposals);
    }

    [Fact]
    public async Task エンジンが無い環境では何もしない()
    {
        await CreateTwoLinesAsync();
        Factory.ShogiEngineGateway.IsEnabled = false;

        await ProcessAllAsync();

        Assert.NotEqual(0, await QueryAsync(db => db.BookPositions.CountAsync(p => !p.CandidatesScanned)));
        Assert.Empty(Factory.ShogiEngineGateway.Judged);
    }
}
