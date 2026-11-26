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

    async Task<(HttpClient Client, Guid PlaybookId, Guid A, Guid B)> CreateTwoLinesAsync()
    {
        var owner = new UserBuilder().Build();
        await SeedAsync(owner);
        var client = CreateClientAs(owner);
        var playbook = await ReadAsync<CreatePlaybookResponse>(await client.PostAsJsonAsync("/api/playbooks", new { name = "角換わり" }));
        var playbookId = playbook.Id.Value;

        async Task<Guid> Play(Guid from, params string[] usis)
        {
            var res = await client.PostAsJsonAsync($"/api/playbooks/{playbookId}/lines", new { lines = new[] { new { fromPositionId = from, usis } } });
            return (await ReadAsync<SavePlaybookLinesResponse>(res)).Positions.Last().PositionId.Value;
        }

        var a = await Play(playbook.RootPositionId.Value, "7g7f", "3c3d");
        var b = await Play(playbook.RootPositionId.Value, "9g9f", "9c9d", "7g7f", "3c3d");
        return (client, playbookId, a, b);
    }

    [Fact]
    public async Task 新しい局面ごとに候補を探して判定し同じなら提案になる()
    {
        var (client, playbookId, a, b) = await CreateTwoLinesAsync();
        Factory.ShogiEngineGateway.Candidates = null;
        Factory.ShogiEngineGateway.Verdict = new EngineVerdict { Kind = ShapeCandidate.VerdictSameWithDelta, Delta = 35, ConfirmedDepth = 6 };

        await ProcessAllAsync();

        Assert.Equal(0, await QueryAsync(db => db.PlaybookPositions.CountAsync(p => !p.CandidatesScanned)));
        Assert.All(await QueryAsync(db => db.ShapeCandidates.ToListAsync()), c => Assert.Equal(ShapeCandidateStatus.Done, c.Status));
        var state = await ReadAsync<GetPlaybookPositionResponse>(await client.GetAsync($"/api/playbooks/{playbookId}/positions/{b}"));
        Assert.Contains(state.Proposals, p => p.OtherPositionId.Value == a && p.Verdict == ShapeCandidate.VerdictSameWithDelta);
        Assert.Equal(0, state.PendingJudgeCount);
    }

    [Fact]
    public async Task 提案をまとめると同じ形のノードになり分けると戻る()
    {
        var (client, playbookId, a, b) = await CreateTwoLinesAsync();
        await ProcessAllAsync();
        var state = await ReadAsync<GetPlaybookPositionResponse>(await client.GetAsync($"/api/playbooks/{playbookId}/positions/{b}"));
        var proposal = state.Proposals.First(p => p.OtherPositionId.Value == a);

        var merge = await client.PostAsync($"/api/playbooks/{playbookId}/candidates/{proposal.CandidateId}/merge", null);

        Assert.Equal(HttpStatusCode.OK, merge.StatusCode);
        var merged = await ReadAsync<GetPlaybookPositionResponse>(await client.GetAsync($"/api/playbooks/{playbookId}/positions/{a}"));
        Assert.Equal(2, merged.Group.Members.Count);
        Assert.Equal(2, merged.Graph.Nodes.Single(n => n.GroupId == merged.Group.Id).PositionCount);

        var split = await client.PostAsync($"/api/playbooks/{playbookId}/positions/{b}/split", null);

        Assert.Equal(HttpStatusCode.OK, split.StatusCode);
        var after = await ReadAsync<GetPlaybookPositionResponse>(await client.GetAsync($"/api/playbooks/{playbookId}/positions/{a}"));
        Assert.Single(after.Group.Members);
    }

    [Fact]
    public async Task 別と判定された候補は提案にならない()
    {
        var (client, playbookId, _, b) = await CreateTwoLinesAsync();
        Factory.ShogiEngineGateway.Verdict = new EngineVerdict { Kind = ShapeCandidate.VerdictDifferent, ConfirmedDepth = 0 };

        await ProcessAllAsync();

        var state = await ReadAsync<GetPlaybookPositionResponse>(await client.GetAsync($"/api/playbooks/{playbookId}/positions/{b}"));
        Assert.Empty(state.Proposals);
    }

    [Fact]
    public async Task 同時実行数の上限で断られた判定は失敗にせず待ち行列に戻す()
    {
        await CreateTwoLinesAsync();
        Factory.ShogiEngineGateway.JudgeException = new HttpRequestException("throttled", null, HttpStatusCode.TooManyRequests);

        await ProcessAllAsync();

        Assert.NotEmpty(Factory.ShogiEngineGateway.Judged);
        Assert.All(await QueryAsync(db => db.ShapeCandidates.ToListAsync()), c => Assert.Equal(ShapeCandidateStatus.Queued, c.Status));
    }

    [Fact]
    public async Task 呼び出しに失敗した判定は失敗として残す()
    {
        await CreateTwoLinesAsync();
        Factory.ShogiEngineGateway.JudgeException = new HttpRequestException("engine crashed");

        await ProcessAllAsync();

        Assert.All(await QueryAsync(db => db.ShapeCandidates.ToListAsync()), c => Assert.Equal(ShapeCandidateStatus.Failed, c.Status));
    }

    [Fact]
    public async Task エンジンが無い環境では何もしない()
    {
        await CreateTwoLinesAsync();
        Factory.ShogiEngineGateway.IsEnabled = false;

        await ProcessAllAsync();

        Assert.NotEqual(0, await QueryAsync(db => db.PlaybookPositions.CountAsync(p => !p.CandidatesScanned)));
        Assert.Empty(Factory.ShogiEngineGateway.Judged);
    }
}
