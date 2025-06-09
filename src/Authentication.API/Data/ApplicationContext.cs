using Authentication.API.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Authentication.API.Data;

public class ApplicationContext(DbContextOptions<ApplicationContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}