using Authentication.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Authentication.API.Data;

public class ApplicationContext(DbContextOptions<ApplicationContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    
    public DbSet<GoogleUser> GoogleUsers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasOne(user => user.GoogleUser)
            .WithOne(googleUser => googleUser.User)
            .HasForeignKey<GoogleUser>(googleUser => googleUser.Id);

        base.OnModelCreating(modelBuilder);
    }
}