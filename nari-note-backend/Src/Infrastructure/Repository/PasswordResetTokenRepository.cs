using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.Repository;
using NariNoteBackend.Domain.ValueObject;
using NariNoteBackend.Infrastructure.Database;

namespace NariNoteBackend.Infrastructure.Repository;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    readonly NariNoteDbContext context;

    public PasswordResetTokenRepository(NariNoteDbContext context)
    {
        this.context = context;
    }

    public async Task<PasswordResetToken> CreateAsync(PasswordResetToken entity)
    {
        context.PasswordResetTokens.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<PasswordResetToken?> FindByIdAsync(PasswordResetTokenId id)
    {
        return await context.PasswordResetTokens.FindAsync(id);
    }

    public async Task<PasswordResetToken> FindForceByIdAsync(PasswordResetTokenId id)
    {
        var entity = await FindByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"PasswordResetToken with ID {id} not found");
        return entity;
    }

    public async Task<PasswordResetToken?> FindByTokenAsync(string token)
    {
        return await context.PasswordResetTokens
            .Include(prt => prt.User)
            .FirstOrDefaultAsync(prt => prt.Token == token);
    }

    public async Task<PasswordResetToken> UpdateAsync(PasswordResetToken entity)
    {
        context.PasswordResetTokens.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(PasswordResetTokenId id)
    {
        var entity = await context.PasswordResetTokens.FindAsync(id);
        if (entity != null)
        {
            context.PasswordResetTokens.Remove(entity);
            await context.SaveChangesAsync();
        }
    }
}
