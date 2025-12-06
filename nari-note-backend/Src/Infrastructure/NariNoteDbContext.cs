using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Infrastructure;

public class NariNoteDbContext : DbContext
{
    public NariNoteDbContext(DbContextOptions<NariNoteDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Article> Articles { get; set; }
}
