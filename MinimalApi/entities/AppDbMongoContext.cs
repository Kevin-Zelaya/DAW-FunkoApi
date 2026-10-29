using Microsoft.EntityFrameworkCore;

public class AppDbMongoContext(
    DbContextOptions options
) : DbContext(options)
{
    public DbSet<Supplier> Suppliers {get; set;}
}