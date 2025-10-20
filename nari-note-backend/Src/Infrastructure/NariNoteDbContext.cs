using Microsoft.EntityFrameworkCore;

namespace nari_note_backend.Infrastructure;

public class NariNoteDbContext : DbContext
{
    public NariNoteDbContext(DbContextOptions<NariNoteDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
}
