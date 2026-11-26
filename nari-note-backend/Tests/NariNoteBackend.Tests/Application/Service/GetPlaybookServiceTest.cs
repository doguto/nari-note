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

public class GetPlaybookServiceTest
{
    [Fact]
    public async Task 開始局面のIDを返す()
    {
        var repository = Substitute.For<IPlaybookRepository>();
        var owner = new UserBuilder().Build();
        var builder = new PlaybookBuilder(owner);
        builder.Line("7g7f");
        var playbook = builder.Build();
        repository.FindForceByIdAsync(playbook.Id).Returns(playbook);
        repository.FindPositionsAsync(playbook.Id).Returns(playbook.Positions);

        var response = await new GetPlaybookService(repository).ExecuteAsync(owner.Id, new GetPlaybookRequest { Id = playbook.Id });

        Assert.Equal(builder.Root.Id, response.RootPositionId);
    }
}
