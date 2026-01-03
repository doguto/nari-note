using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Domain.Entity;

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

    public async Task<User> FindForceByIdAsync(int id)
    {
        var user = await FindByIdAsync(id);
        if (user == null) throw new KeyNotFoundException($"User with ID {id} not found");

        return user;
    }

    public async Task<User> UpdateAsync(User entity)
    {
        context.Users.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(int id)
    {
        var user = await context.Users.FindAsync(id);
        if (user != null)
        {
            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> FindByUsernameOrEmailAsync(string usernameOrEmail)
    {
        return await context.Users
            .FirstOrDefaultAsync(u => u.Name == usernameOrEmail || u.Email == usernameOrEmail);
    }
    
    public async Task<User> CreateAsync(User user)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }
}
