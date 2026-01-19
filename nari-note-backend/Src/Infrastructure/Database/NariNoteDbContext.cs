using Microsoft.EntityFrameworkCore;
using NariNoteBackend.Domain.Entity;
using NariNoteBackend.Domain.ValueObject;

namespace NariNoteBackend.Infrastructure.Database;

public class NariNoteDbContext : DbContext
{
    public NariNoteDbContext(DbContextOptions<NariNoteDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ArticleTag> ArticleTags { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Follow> Follows { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Article>(x =>
        {
            x.HasOne(a => a.Author)
             .WithMany(u => u.Articles)
             .HasForeignKey(a => a.AuthorId)
             .OnDelete(DeleteBehavior.Cascade);

            x.HasMany(a => a.Likes)
             .WithOne(l => l.Article)
             .HasForeignKey(l => l.ArticleId)
             .OnDelete(DeleteBehavior.Cascade);

            x.HasMany(a => a.Comments)
             .WithOne(c => c.Article)
             .HasForeignKey(c => c.ArticleId)
             .OnDelete(DeleteBehavior.Cascade);

            x.Property(a => a.Id)
             .HasValueGenerator<ArticleIdValueGenerator>();

            x.Property(a => a.Id)
             .HasVogenConversion();

            x.Property(a => a.AuthorId)
             .HasVogenConversion();
        });

        modelBuilder.Entity<User>(x =>
        {
            x.HasMany(u => u.Articles)
             .WithOne(a => a.Author)
             .HasForeignKey(a => a.AuthorId)
             .OnDelete(DeleteBehavior.Cascade);

            x.HasMany(u => u.Likes)
             .WithOne(l => l.User)
             .HasForeignKey(l => l.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            x.HasMany(u => u.Comments)
             .WithOne(c => c.User)
             .HasForeignKey(c => c.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            x.HasMany(u => u.Followings)
             .WithOne(f => f.Follower)
             .HasForeignKey(f => f.FollowerId)
             .OnDelete(DeleteBehavior.Restrict);

            x.HasMany(u => u.Followers)
             .WithOne(f => f.Following)
             .HasForeignKey(f => f.FollowingId)
             .OnDelete(DeleteBehavior.Restrict);

            x.HasMany(u => u.Notifications)
             .WithOne(n => n.User)
             .HasForeignKey(n => n.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            x.Property(u => u.Id)
             .HasValueGenerator<UserIdValueGenerator>();

            x.Property(u => u.Id)
             .HasVogenConversion();
        });

        modelBuilder.Entity<Tag>(x =>
        {
            x.HasMany(t => t.ArticleTags)
             .WithOne(at => at.Tag)
             .HasForeignKey(at => at.TagId)
             .OnDelete(DeleteBehavior.Cascade);

            x.Property(t => t.Id)
             .HasValueGenerator<TagIdValueGenerator>();

            x.Property(t => t.Id)
             .HasVogenConversion();
        });

        modelBuilder.Entity<ArticleTag>(x =>
        {
            x.HasOne(at => at.Article)
             .WithMany(a => a.ArticleTags)
             .HasForeignKey(at => at.ArticleId)
             .OnDelete(DeleteBehavior.Cascade);

            x.HasOne(at => at.Tag)
             .WithMany(t => t.ArticleTags)
             .HasForeignKey(at => at.TagId)
             .OnDelete(DeleteBehavior.Cascade);

            x.Property(at => at.Id)
             .HasValueGenerator<ArticleTagIdValueGenerator>();

            x.Property(at => at.Id)
             .HasVogenConversion();

            x.Property(at => at.ArticleId)
             .HasVogenConversion();

            x.Property(at => at.TagId)
             .HasVogenConversion();
        });

        modelBuilder.Entity<Like>(x =>
        {
            x.HasOne(l => l.User)
             .WithMany(u => u.Likes)
             .HasForeignKey(l => l.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            x.HasOne(l => l.Article)
             .WithMany(a => a.Likes)
             .HasForeignKey(l => l.ArticleId)
             .OnDelete(DeleteBehavior.Cascade);

            x.Property(l => l.Id)
             .HasValueGenerator<LikeIdValueGenerator>();

            x.Property(l => l.Id)
             .HasVogenConversion();

            x.Property(l => l.UserId)
             .HasVogenConversion();

            x.Property(l => l.ArticleId)
             .HasVogenConversion();
        });

        modelBuilder.Entity<Comment>(x =>
        {
            x.HasOne(c => c.User)
             .WithMany(u => u.Comments)
             .HasForeignKey(c => c.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            x.HasOne(c => c.Article)
             .WithMany(a => a.Comments)
             .HasForeignKey(c => c.ArticleId)
             .OnDelete(DeleteBehavior.Cascade);

            x.Property(c => c.Id)
             .HasValueGenerator<CommentIdValueGenerator>();

            x.Property(c => c.Id)
             .HasVogenConversion();

            x.Property(c => c.UserId)
             .HasVogenConversion();

            x.Property(c => c.ArticleId)
             .HasVogenConversion();
        });

        modelBuilder.Entity<Follow>(x =>
        {
            x.HasOne(f => f.Follower)
             .WithMany(u => u.Followings)
             .HasForeignKey(f => f.FollowerId)
             .OnDelete(DeleteBehavior.Restrict);

            x.HasOne(f => f.Following)
             .WithMany(u => u.Followers)
             .HasForeignKey(f => f.FollowingId)
             .OnDelete(DeleteBehavior.Restrict);

            x.Property(f => f.Id)
             .HasValueGenerator<FollowIdValueGenerator>();

            x.Property(f => f.Id)
             .HasVogenConversion();

            x.Property(f => f.FollowerId)
             .HasVogenConversion();

            x.Property(f => f.FollowingId)
             .HasVogenConversion();
        });

        modelBuilder.Entity<Notification>(x =>
        {
            x.HasOne(n => n.User)
             .WithMany(u => u.Notifications)
             .HasForeignKey(n => n.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            x.HasOne(n => n.Article)
             .WithMany()
             .HasForeignKey(n => n.ArticleId)
             .OnDelete(DeleteBehavior.Cascade);

            x.Property(n => n.Id)
             .HasValueGenerator<NotificationIdValueGenerator>();

            x.Property(n => n.Id)
             .HasVogenConversion();

            x.Property(n => n.UserId)
             .HasVogenConversion();

            x.Property(n => n.ArticleId)
             .HasVogenConversion();
        });
    }
}
