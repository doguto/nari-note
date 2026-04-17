using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Infrastructure.Database;

namespace NariNoteBackend.Infrastructure.Repository;

public class EmailVerificationRepository : IEmailVerificationRepository
{
    readonly NariNoteDbContext context;

    public EmailVerificationRepository(NariNoteDbContext context)
    {
        this.context = context;
    }

    public async Task<EmailVerification> CreateAsync(EmailVerification entity)
    {
        context.EmailVerifications.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<EmailVerification?> FindByIdAsync(EmailVerificationId id)
    {
        return await context.EmailVerifications.FindAsync(id);
    }

    public async Task<EmailVerification> FindForceByIdAsync(EmailVerificationId id)
    {
        var entity = await FindByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"EmailVerification with ID {id} not found");
        return entity;
    }

    public async Task<EmailVerification?> FindByTokenAsync(string token)
    {
        return await context.EmailVerifications
            .Include(ev => ev.User)
            .FirstOrDefaultAsync(ev => ev.Token == token);
    }

    public async Task<EmailVerification> UpdateAsync(EmailVerification entity)
    {
        context.EmailVerifications.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(EmailVerificationId id)
    {
        var entity = await context.EmailVerifications.FindAsync(id);
        if (entity != null)
        {
            context.EmailVerifications.Remove(entity);
            await context.SaveChangesAsync();
        }
    }
}
