using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Application.Repository;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Infrastructure.Repository;

public class UserRepository : IUserRepository
{
    private readonly NariNoteDbContext context;
    
    public UserRepository(NariNoteDbContext context)
    {
        this.context = context;
    }
    
    public async Task<User?> FindByIdAsync(int id)
    {
        return await this.context.Users.FindAsync(id);
    }
}
