namespace NariNoteBackend.Domain.Gateway;

public record EmailMessage(
    string From,
    IList<string> To,
    string Subject,
    string? HtmlBody = null,
    string? TextBody = null
);
