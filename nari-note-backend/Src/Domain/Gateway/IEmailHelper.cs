namespace NariNoteBackend.Domain.Gateway;

public interface IEmailHelper
{
    Task SendAsync(EmailMessage message);
}
