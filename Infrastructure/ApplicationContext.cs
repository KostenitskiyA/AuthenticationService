using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class ApplicationContext(DbContextOptions<ApplicationContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    public DbSet<GoogleUser> GoogleUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(user => user.Name)
                .IsRequired()
                .HasMaxLength(63);

            entity.Property(user => user.Email)
                .IsRequired()
                .HasMaxLength(63);

            entity.HasIndex(user => user.Email)
                .IsUnique();

            entity.HasOne(user => user.GoogleUser)
                .WithOne(googleUser => googleUser.User)
                .HasForeignKey<GoogleUser>(googleUser => googleUser.Id);
        });


        base.OnModelCreating(modelBuilder);
    }
}