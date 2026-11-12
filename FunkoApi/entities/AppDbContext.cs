using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users {get; set;}
    public DbSet<Funko> Funkos {get; set;}
    public DbSet<Category> Categories {get; set;}

     protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        PosgreContextConfiguration.Configure(modelBuilder);
    }
    
}