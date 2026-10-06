namespace NariNoteBackend.Application.BackgroundJob;

public interface ISignUpJobQueue
{
    ValueTask EnqueueAsync(SignUpJob job, CancellationToken cancellationToken = default);
    ValueTask<SignUpJob> DequeueAsync(CancellationToken cancellationToken);
}
