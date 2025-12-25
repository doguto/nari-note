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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Article関係の設定
        modelBuilder.Entity<Article>()
            .HasOne(a => a.Author)
            .WithMany(u => u.Articles)
            .HasForeignKey(a => a.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Article>()
            .HasMany(a => a.Likes)
            .WithOne(l => l.Article)
            .HasForeignKey(l => l.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        // User関係の設定
        modelBuilder.Entity<User>()
            .HasMany(u => u.Sessions)
            .WithOne(s => s.User)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Likes)
            .WithOne(l => l.User)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Notifications)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Follow関係の設定（自己参照）
        modelBuilder.Entity<Follow>()
            .HasOne(f => f.Follower)
            .WithMany(u => u.Followings)
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Follow>()
            .HasOne(f => f.Following)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FollowingId)
            .OnDelete(DeleteBehavior.Restrict);

        // ArticleTag（多対多の中間テーブル）の設定
        modelBuilder.Entity<ArticleTag>()
            .HasOne(at => at.Article)
            .WithMany(a => a.ArticleTags)
            .HasForeignKey(at => at.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ArticleTag>()
            .HasOne(at => at.Tag)
            .WithMany(t => t.ArticleTags)
            .HasForeignKey(at => at.TagId)
            .OnDelete(DeleteBehavior.Cascade);

        // Notification関係の設定
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.Article)
            .WithMany()
            .HasForeignKey(n => n.ArticleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
