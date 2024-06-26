using Microsoft.EntityFrameworkCore;
using Models;

namespace Backend.Data
{
    public class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Image> Images { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure User
            modelBuilder.Entity<User>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<User>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<User>()
                .Property(u => u.Password)
                .IsRequired();

            // Configure Image
            modelBuilder.Entity<Image>()
                .HasKey(i => i.ImageId);

            modelBuilder.Entity<Image>()
                .Property(i => i.FileName)
                .IsRequired();

            modelBuilder.Entity<Image>()
                .Property(i => i.Url)
                .IsRequired();

            modelBuilder.Entity<Image>()
                .Property(i => i.UploadDate)
                .IsRequired();

            modelBuilder.Entity<Image>()
                .Property(i => i.IsPublic)
                .IsRequired();

            modelBuilder.Entity<Image>()
                .Property(i => i.IsDeleted)
                .IsRequired();

            modelBuilder.Entity<Image>()
                .HasOne(i => i.User)
                .WithMany(u => u.Images)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
