using NariNoteBackend.Application.Dto.Request;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Service;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Gateway;
using NariNoteBackend.Domain.ReadModel;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.Shogi;
using NariNoteBackend.Tests.Support;
using NariNoteBackend.Tests.Support.Builder;
using NSubstitute;

namespace NariNoteBackend.Tests.Application.Service;

public class RejectShapeCandidateServiceTest
{
    [Fact]
    public async Task 候補を別のままにする()
    {
        var repository = Substitute.For<IPlaybookRepository>();
        var owner = new UserBuilder().Build();
        var builder = new PlaybookBuilder(owner);
        var candidate = builder.Candidate(builder.Line("7g7f"), builder.Line("2g2f"));
        var playbook = builder.Build();
        repository.FindForceGraphByIdAsync(playbook.Id).Returns(playbook);

        await new RejectShapeCandidateService(repository).ExecuteAsync(
            owner.Id,
            new RejectShapeCandidateRequest { Id = playbook.Id, CandidateId = candidate.Id }
        );

        Assert.Equal(ShapeCandidateDecision.Rejected, candidate.Decision);
    }
}
