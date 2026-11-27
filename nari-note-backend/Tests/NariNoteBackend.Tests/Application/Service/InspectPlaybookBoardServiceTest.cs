using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Dto.Response;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class InspectPlaybookBoardServiceTest
{
    readonly IPlaybookRepository repository = Substitute.For<IPlaybookRepository>();
    readonly User owner = new UserBuilder().Build();
    readonly PlaybookBuilder builder;
    readonly InspectPlaybookBoardService service;

    public InspectPlaybookBoardServiceTest()
    {
        this.builder = new PlaybookBuilder(this.owner);
        var playbook = this.builder.Build();
        this.repository.FindForceGraphByIdAsync(playbook.Id).Returns(playbook);
        this.service = new InspectPlaybookBoardService(this.repository);
    }

    Task<InspectPlaybookBoardResponse> Inspect(string sfen) =>
        this.service.ExecuteAsync(this.owner.Id, new InspectPlaybookBoardRequest { Id = this.builder.Build().Id, Sfen = sfen });

    [Fact]
    public async Task 端歩だけ違うノードを似た盤面として返す()
    {
        var a = this.builder.Line("7g7f", "3c3d", "2g2f");
        var b = this.builder.Line("9g9f", "9c9d", "7g7f", "3c3d", "2g2f");
        var boardA = this.builder.Name(a, "角道を開けた形");
        var boardB = this.builder.Name(b, "端歩を突き合った形");

        var response = await this.Inspect(b);

        Assert.Equal(boardB.Id, response.BoardId);
        var similar = Assert.Single(response.Similar);
        Assert.Equal(boardA.Id, similar.BoardId);
        Assert.Equal("角道を開けた形", similar.NodeName);
    }

    [Fact]
    public async Task ノードに属さない局面は名前の候補だけを返す()
    {
        this.builder.Line("7g7f", "3c3d", "2g2f");
        var sfen = PlaybookBuilder.Play(this.builder.Build().RootSfen, "7g7f");

        var response = await this.Inspect(sfen);

        Assert.Equal(this.builder.BoardAt(sfen)!.Id, response.BoardId);
        Assert.Empty(response.Members);
        Assert.Empty(response.Similar);
    }

    [Fact]
    public async Task 定跡に無い局面は扱わない()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => this.Inspect(PlaybookBuilder.Play(this.builder.Build().RootSfen, "2g2f")));
    }

    [Fact]
    public async Task まとめた局面の違いから遷移の条件の候補を出す()
    {
        this.builder.Line("7g7f", "3c3d", "2g2f", "4a3b");
        this.builder.Line("9g9f", "9c9d", "7g7f", "3c3d", "2g2f", "4a3b");
        var boardA = this.builder.Name(PlaybookBuilder.Play(this.builder.Build().RootSfen, "7g7f", "3c3d", "2g2f"), "角道");
        var boardB = this.builder.Name(PlaybookBuilder.Play(this.builder.Build().RootSfen, "9g9f", "9c9d", "7g7f", "3c3d", "2g2f"), "角道（端歩）");
        this.builder.Merge(boardA, boardB);

        var response = await this.Inspect(boardB.Sfen);

        Assert.Equal(2, response.Members.Count);
        Assert.Contains(response.Members, m => m.BoardId == boardA.Id && m.IsRepresentative);
        var suggestions = Assert.Single(response.ConditionSuggestions).Suggestions;
        Assert.NotEmpty(suggestions);
    }
}
