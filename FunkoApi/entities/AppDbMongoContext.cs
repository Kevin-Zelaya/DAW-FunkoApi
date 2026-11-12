using Microsoft.EntityFrameworkCore;

public class AppDbMongoContext : DbContext
{
    public DbSet<Supplier> Suppliers {get; set;}
    public AppDbMongoContext(DbContextOptions<AppDbMongoContext> options) : base(options)
    {

        Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        MongoContextConfiguration.Configure(modelBuilder);
    }
    
    
}