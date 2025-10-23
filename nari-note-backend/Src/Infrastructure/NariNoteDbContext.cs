using Microsoft.EntityFrameworkCore;

namespace NariNoteBackend.Infrastructure;

public class NariNoteDbContext : DbContext
{
    public NariNoteDbContext(DbContextOptions<NariNoteDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
}
