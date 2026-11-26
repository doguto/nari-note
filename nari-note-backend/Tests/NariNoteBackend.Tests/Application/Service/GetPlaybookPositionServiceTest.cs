using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class GetPlaybookPositionServiceTest
{
    readonly IPlaybookRepository playbookRepository = Substitute.For<IPlaybookRepository>();
    readonly IShogiEngineGateway shogiEngineGateway = Substitute.For<IShogiEngineGateway>();
    readonly User owner = new UserBuilder().Build();
    readonly PlaybookBuilder builder;
    readonly Playbook playbook;
    readonly GetPlaybookPositionService service;

    public GetPlaybookPositionServiceTest()
    {
        this.builder = new PlaybookBuilder(this.owner);
        this.playbook = this.builder.Build();
        this.playbookRepository.FindForceGraphByIdAsync(this.playbook.Id).Returns(this.playbook);
        this.shogiEngineGateway.IsEnabled.Returns(true);
        this.service = new GetPlaybookPositionService(this.playbookRepository, this.shogiEngineGateway);
    }

    Task<NariNoteBackend.Application.Dto.Response.GetPlaybookPositionResponse> Get(PlaybookPosition position) =>
        this.service.ExecuteAsync(this.owner.Id, new GetPlaybookPositionRequest { Id = this.playbook.Id, PositionId = position.Id });

    [Fact]
    public async Task 開始局面からの手順と次の一手を記譜で返す()
    {
        var current = this.builder.Line("7g7f", "3c3d", "8h2b+");
        this.builder.Play(current, "3a2b");
        this.playbook.Moves.Last().Memo = "取り返す一手";

        var response = await this.Get(current);

        Assert.Equal(["▲７六歩", "△３四歩", "▲２二角成"], response.Path.Select(p => p.Label));
        Assert.Equal("8h2b+", response.LastMoveUsi);
        Assert.False(response.IsSenteTurn);
        var next = Assert.Single(response.Moves);
        Assert.Equal("△同　銀", next.Label);
        Assert.Equal("取り返す一手", next.Memo);
        Assert.Contains("3a2b", response.LegalMoves);
    }

    [Fact]
    public async Task 形の名前の候補を盤面から作る()
    {
        var response = await this.Get(this.builder.Root);

        Assert.Equal(["平手"], response.Group.NameSuggestions);
    }

    [Fact]
    public async Task 同じ手で行き先が分かれる局面との違いを辺の条件の候補にする()
    {
        var a = this.builder.Line("7g7f", "3c3d");
        var b = this.builder.Line("9g9f", "9c9d", "7g7f", "3c3d");
        this.builder.Merge(a, b);
        this.builder.Play(a, "2g2f");
        this.builder.Play(b, "2g2f");
        this.playbook.Moves.Last().Condition = "9筋を突き合っている";

        var response = await this.Get(a);

        var move = Assert.Single(response.Moves);
        Assert.Equal("9筋を突いていない", move.ConditionSuggestions[0]);
        var edges = response.Graph.Edges.Where(e => e.Label == "▲２六歩").ToList();
        Assert.All(edges, e => Assert.True(e.IsConditional));
        Assert.Equal(["", "9筋を突き合っている"], edges.Select(e => e.Condition).Order());
    }

    [Fact]
    public async Task まとめた局面は現在の局面との違いを添えて返す()
    {
        var a = this.builder.Line("7g7f", "3c3d");
        var b = this.builder.Line("9g9f", "9c9d", "7g7f", "3c3d");
        this.builder.Merge(a, b);

        var response = await this.Get(a);

        Assert.Equal(2, response.Group.Members.Count);
        var other = response.Group.Members.Single(m => !m.IsCurrent);
        Assert.Equal(b.Id, other.PositionId);
        Assert.Equal("△歩 ９三→９四、▲歩 ９七→９六", other.Diff);
        Assert.True(response.Group.Members.Single(m => m.IsCurrent).IsRepresentative);
    }

    [Fact]
    public async Task 同じ形の候補は判定が同じ系統で未決定のものだけを相手の局面として返す()
    {
        var a = this.builder.Line("7g7f", "3c3d");
        var b = this.builder.Line("9g9f", "9c9d", "7g7f", "3c3d");
        var c = this.builder.Line("1g1f", "1c1d", "7g7f", "3c3d");
        var d = this.builder.Line("2g2f", "8c8d");
        var proposal = this.builder.Candidate(a, b, ShapeCandidate.VerdictSameWithDelta, 40);
        this.builder.Candidate(a, c, ShapeCandidate.VerdictDifferent);
        this.builder.Candidate(a, d).Decision = ShapeCandidateDecision.Rejected;

        var fromA = await this.Get(a);
        var fromB = await this.Get(b);

        var item = Assert.Single(fromA.Proposals);
        Assert.Equal(proposal.Id, item.CandidateId);
        Assert.Equal(b.Id, item.OtherPositionId);
        Assert.Equal(40, item.Delta);
        // 相手側から見ると評価値の差は符号が逆になる
        Assert.Equal(-40, Assert.Single(fromB.Proposals).Delta);
    }

    [Fact]
    public async Task 同じ形から同じ手で行き先が分かれる枝は条件付きになる()
    {
        var a = this.builder.Line("7g7f", "3c3d");
        var b = this.builder.Line("9g9f", "9c9d", "7g7f", "3c3d");
        this.builder.Merge(a, b);
        this.builder.Play(a, "2g2f");
        this.builder.Play(b, "2g2f");

        var response = await this.Get(a);

        var branches = response.Graph.Edges.Where(e => e.FromGroupId == a.ShapeGroupId).ToList();
        Assert.Equal(2, branches.Count);
        Assert.All(branches, e => Assert.True(e.IsConditional));
        var node = response.Graph.Nodes.Single(n => n.GroupId == a.ShapeGroupId);
        Assert.Equal(2, node.PositionCount);
        Assert.False(node.IsLeaf);
    }

    [Fact]
    public async Task エンジンが無い環境では判定待ちの件数を0にする()
    {
        this.playbookRepository.CountPendingJudgesAsync(this.playbook.Id).Returns(5);

        var enabled = await this.Get(this.builder.Root);
        this.shogiEngineGateway.IsEnabled.Returns(false);
        var disabled = await this.Get(this.builder.Root);

        Assert.Equal(5, enabled.PendingJudgeCount);
        Assert.Equal(0, disabled.PendingJudgeCount);
    }

    [Fact]
    public async Task 持ち主以外は見られない()
    {
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            this.service.ExecuteAsync(new UserBuilder().Build().Id, new GetPlaybookPositionRequest { Id = this.playbook.Id, PositionId = this.builder.Root.Id })
        );
    }
}
