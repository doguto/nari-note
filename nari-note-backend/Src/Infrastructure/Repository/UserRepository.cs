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
        try
        {
            return await this.context.Users.FindAsync(id);
        }
        catch (System.Exception ex)
        {
            throw new InfrastructureException(
                $"Error occurred while fetching user with ID {id}",
                ex);
        }
    }
}
