using NariNoteBackend.Domain.Gateway;
using Resend;
using EmailMessage = NariNoteBackend.Domain.Gateway.EmailMessage;
using ResendMessage = Resend.EmailMessage;

namespace NariNoteBackend.Infrastructure.Gateway;

public class ResendEmailHelper : IEmailHelper
{
    readonly IResend resend;

    public ResendEmailHelper(IResend resend)
    {
        this.resend = resend;
    }

    public async Task SendAsync(EmailMessage message)
    {
        var resendMessage = new ResendMessage
        {
            From = message.From,
            To = ConvertStringListToEmailAddressList(message.To.ToList()),
            Subject = message.Subject,
            HtmlBody = message.HtmlBody,
            TextBody = message.TextBody
        };
        await resend.EmailSendAsync(resendMessage);
    }

    EmailAddressList ConvertStringListToEmailAddressList(List<string> emailAddresses)
    {
        var emailAddressList = new EmailAddressList();
        foreach (var emailAddress in emailAddresses) emailAddressList.Add(emailAddress);
        return emailAddressList;
    }
}
