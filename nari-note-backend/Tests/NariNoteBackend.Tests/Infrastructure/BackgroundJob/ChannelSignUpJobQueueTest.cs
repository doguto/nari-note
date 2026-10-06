using NariNoteBackend.Application.BackgroundJob;
using NariNoteBackend.Infrastructure.BackgroundJob;

namespace NariNoteBackend.Tests.Infrastructure.BackgroundJob;

public class ChannelSignUpJobQueueTest
{
    [Fact]
    public async Task 投入した順にジョブを取り出せる()
    {
        var queue = new ChannelSignUpJobQueue();
        var first = new SignUpJob("a", "a@example.com", "hash-a");
        var second = new SignUpJob("b", "b@example.com", "hash-b");

        await queue.EnqueueAsync(first);
        await queue.EnqueueAsync(second);

        Assert.Equal(first, await queue.DequeueAsync(CancellationToken.None));
        Assert.Equal(second, await queue.DequeueAsync(CancellationToken.None));
    }
}
