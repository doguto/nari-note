using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class MergeShapeCandidateServiceTest
{
    readonly IBookRepository bookRepository = Substitute.For<IBookRepository>();
    readonly User owner = new UserBuilder().Build();
    readonly BookBuilder builder;
    readonly Book book;
    readonly BookPosition a;
    readonly BookPosition b;
    readonly MergeShapeCandidateService service;

    public MergeShapeCandidateServiceTest()
    {
        this.builder = new BookBuilder(this.owner);
        this.book = this.builder.Build();
        this.a = this.builder.Line("7g7f", "3c3d");
        this.b = this.builder.Line("9g9f", "9c9d", "7g7f", "3c3d");
        this.bookRepository.FindForceGraphByIdAsync(this.book.Id).Returns(this.book);
        this.service = new MergeShapeCandidateService(this.bookRepository, new TestTimeProvider());
    }

    [Fact]
    public async Task まとめると相手の形の局面がこちらの形に入りメモがつながる()
    {
        this.a.ShapeGroup.Memo = "角道を開け合った形";
        this.b.ShapeGroup.Memo = "端歩の突き合いあり";
        this.b.ShapeGroup.Name = "端歩型";
        var dropped = this.b.ShapeGroup;
        var candidate = this.builder.Candidate(this.a, this.b);

        await this.service.ExecuteAsync(this.owner.Id, new MergeShapeCandidateRequest { Id = this.book.Id, CandidateId = candidate.Id });

        Assert.Equal(this.a.ShapeGroupId, this.b.ShapeGroupId);
        Assert.Equal("角道を開け合った形\n端歩の突き合いあり", this.a.ShapeGroup.Memo);
        Assert.Equal("端歩型", this.a.ShapeGroup.Name);
        Assert.Equal(ShapeCandidateDecision.Merged, candidate.Decision);
        this.bookRepository.Received(1).RemoveShapeGroup(dropped);
        await this.bookRepository.Received(1).SaveAsync();
    }

    [Fact]
    public async Task 判定が終わっていない候補はまとめられない()
    {
        var candidate = this.builder.Candidate(this.a, this.b);
        candidate.Status = ShapeCandidateStatus.Queued;

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            this.service.ExecuteAsync(this.owner.Id, new MergeShapeCandidateRequest { Id = this.book.Id, CandidateId = candidate.Id })
        );
    }

    [Fact]
    public async Task まとめた研究メモが上限を超えるならまとめない()
    {
        this.a.ShapeGroup.Memo = new string('あ', 6000);
        this.b.ShapeGroup.Memo = new string('い', 6000);
        var candidate = this.builder.Candidate(this.a, this.b);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            this.service.ExecuteAsync(this.owner.Id, new MergeShapeCandidateRequest { Id = this.book.Id, CandidateId = candidate.Id })
        );
        Assert.NotEqual(this.a.ShapeGroupId, this.b.ShapeGroupId);
    }

    [Fact]
    public async Task 持ち主以外はまとめられない()
    {
        var candidate = this.builder.Candidate(this.a, this.b);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            this.service.ExecuteAsync(new UserBuilder().Build().Id, new MergeShapeCandidateRequest { Id = this.book.Id, CandidateId = candidate.Id })
        );
    }
}
