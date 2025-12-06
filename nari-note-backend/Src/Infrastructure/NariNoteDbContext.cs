using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain;

namespace NariNoteBackend.Infrastructure;

public class NariNoteDbContext : DbContext
{
    public NariNoteDbContext(DbContextOptions<NariNoteDbContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<Session> Sessions { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ArticleTag> ArticleTags { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<Notification> Notifications { get; set; }
}
