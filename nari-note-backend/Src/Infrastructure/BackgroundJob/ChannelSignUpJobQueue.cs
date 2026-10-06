using System.Threading.Channels;
using NariNoteBackend.Application.BackgroundJob;

namespace NariNoteBackend.Infrastructure.BackgroundJob;

public class ChannelSignUpJobQueue : ISignUpJobQueue
{
    const int Capacity = 1000;

    readonly Channel<SignUpJob> channel = Channel.CreateBounded<SignUpJob>(
        new BoundedChannelOptions(Capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true
        }
    );

    public ValueTask EnqueueAsync(SignUpJob job, CancellationToken cancellationToken = default)
    {
        return this.channel.Writer.WriteAsync(job, cancellationToken);
    }

    public ValueTask<SignUpJob> DequeueAsync(CancellationToken cancellationToken)
    {
        return this.channel.Reader.ReadAsync(cancellationToken);
    }
}
