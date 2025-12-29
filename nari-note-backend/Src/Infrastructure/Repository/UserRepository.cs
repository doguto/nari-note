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
        try
        {
            return await context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }
        catch (System.Exception ex)
        {
            throw new InfrastructureException(
                $"Error occurred while fetching user with email {email}", ex);
        }
    }
    
    public async Task<User?> FindByUsernameOrEmailAsync(string usernameOrEmail)
    {
        try
        {
            return await context.Users
                .FirstOrDefaultAsync(u => u.Name == usernameOrEmail || u.Email == usernameOrEmail);
        }
        catch (System.Exception ex)
        {
            throw new InfrastructureException(
                $"Error occurred while fetching user with username or email {usernameOrEmail}", ex);
        }
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
                throw new ConflictException("User with this email already exists", ex);
            }
            throw new InfrastructureException(
                "Database error occurred while creating user", ex);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflictException("The user was modified by another user", ex);
        }
    }
}
