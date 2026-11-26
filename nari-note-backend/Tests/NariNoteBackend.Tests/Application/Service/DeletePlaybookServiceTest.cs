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

public class DeletePlaybookServiceTest
{
    [Fact]
    public async Task 持ち主だけが削除できる()
    {
        var repository = Substitute.For<IPlaybookRepository>();
        var owner = new UserBuilder().Build();
        var playbook = new PlaybookBuilder(owner).Build();
        repository.FindForceByIdAsync(playbook.Id).Returns(playbook);
        var service = new DeletePlaybookService(repository);

        await Assert.ThrowsAsync<ForbiddenException>(() => service.ExecuteAsync(new UserBuilder().Build().Id, new DeletePlaybookRequest { Id = playbook.Id }));
        await service.ExecuteAsync(owner.Id, new DeletePlaybookRequest { Id = playbook.Id });

        await repository.Received(1).DeleteAsync(playbook.Id);
    }
}
