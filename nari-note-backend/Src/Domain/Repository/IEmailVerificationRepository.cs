using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface IEmailVerificationRepository : IRepository<EmailVerification, EmailVerificationId>
{
    Task<EmailVerification?> FindByTokenAsync(string token);
}
