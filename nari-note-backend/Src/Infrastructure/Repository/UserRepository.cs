using Microsoft.EntityFrameworkCore;
using Npgsql;
using NariNoteBackend.Application.Exception;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Infrastructure.Repository;

public class UserRepository : IUserRepository
{
    readonly NariNoteDbContext context;
    
    public UserRepository(NariNoteDbContext context)
    {
        this.context = context;
    }
    
    public async Task<User> CreateAsync(User user)
    {
        try
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx)
        {
            if (pgEx.SqlState == "23505") // Unique constraint violation
            {
                throw new ConflictException("このメールアドレスは既に使用されています", ex);
            }
            throw new InfrastructureException(
                "Database error occurred while creating user", ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException("The user was modified by another user", ex);
        }
    }
    
    public async Task<User?> FindByIdAsync(int id)
    {
        return await context.Users.FindAsync(id);
    }
    
    public async Task<User> GetByIdAsync(int id)
    {
        var user = await FindByIdAsync(id);
        
        if (user == null)
        {
            throw new NotFoundException($"User with ID {id} not found");
        }
        
        return user;
    }
    
    public async Task<User?> FindByEmailAsync(string email)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }
    
    public async Task<User> GetByEmailAsync(string email)
    {
        var user = await FindByEmailAsync(email);
        
        if (user == null)
        {
            throw new NotFoundException($"User with email {email} not found");
        }
        
        return user;
    }
}
