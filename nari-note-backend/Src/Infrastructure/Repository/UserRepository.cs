using Microsoft.EntityFrameworkCore;
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
}
