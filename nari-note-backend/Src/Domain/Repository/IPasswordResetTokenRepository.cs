using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Domain.Repository;

public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken, PasswordResetTokenId>
{
    Task<PasswordResetToken?> FindByTokenAsync(string token);
}
