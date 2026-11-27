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

public class GetMyPlaybooksServiceTest
{
    [Fact]
    public async Task 自分の定跡の一覧を返す()
    {
        var repository = Substitute.For<IPlaybookRepository>();
        var owner = new UserBuilder().Build();
        var summary = new PlaybookSummary
        {
            Id = NariNoteBackend.Domain.ValueObject.PlaybookId.From(Guid.CreateVersion7()),
            Name = "角換わり",
            UpdatedAt = TestTimeProvider.DefaultUtcNow,
        };
        repository.FindSummariesByUserAsync(owner.Id).Returns([summary]);

        var response = await new GetMyPlaybooksService(repository).ExecuteAsync(owner.Id);

        var playbook = Assert.Single(response.Playbooks);
        Assert.Equal("角換わり", playbook.Name);
    }
}
