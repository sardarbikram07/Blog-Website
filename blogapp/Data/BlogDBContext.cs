
using blogapp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace blogapp.Data
{
    public class BlogDBContext : IdentityDbContext<IdentityUser>
    {
        public BlogDBContext(DbContextOptions<BlogDBContext> options) : base(options) { }

        public DbSet<BlogPost> BlogPosts { get; set; }
        public new DbSet<User> Users { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Must call base for Identity schema

            // LIKEs Config
            modelBuilder.Entity<Like>()
                .HasOne(l => l.BlogPost)
                .WithMany(p => p.Likes)
                .HasForeignKey(l => l.BlogPostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // BOOKMARKs Config
            modelBuilder.Entity<Bookmark>()
                .HasOne(b => b.Post)
                .WithMany(p => p.Bookmarks)
                .HasForeignKey(b => b.PostId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bookmark>()
                .HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // COMMENT replies
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.Replies)
                .HasForeignKey(c => c.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // USER Config
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.ProfileImagePath).IsRequired(false);
                entity.Property(e => e.ResetToken).IsRequired(false);
                entity.Property(e => e.ResetTokenExpiry).IsRequired(false);
                entity.Property(e => e.DateTime).IsRequired();
                entity.Property(e => e.BlogAccessStatus).IsRequired().HasDefaultValue(AccessStatus.Pending);
            });

            // USER NOTIFICATION Config
            modelBuilder.Entity<UserNotification>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(un => un.User)
                    .WithMany(u => u.UserNotifications)
                    .HasForeignKey(un => un.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(un => un.Notification)
                    .WithMany(n => n.UserNotifications)
                    .HasForeignKey(un => un.NotificationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
