namespace NariNoteBackend.Domain.Gateway;

public interface IEmailGateway
{
    Task SendAsync(EmailMessage message);
}
