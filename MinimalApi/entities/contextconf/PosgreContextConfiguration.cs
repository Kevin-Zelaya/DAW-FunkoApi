using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Serilog;

public abstract class PosgreContextConfiguration
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        Log.Debug("[DBCONTEXT-CONFIG] Configurando entidades para el proveedor 'PosgreSql'");
        modelBuilder.Entity<Funko>(entity =>
        {
            ///<summary> Generar id </summary>
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Id).ValueGeneratedOnAdd();

            ///<summary> Nombre como indice </summary>
            entity.HasIndex(p => p.Name)
            .IsUnique();
            entity.Property(p => p.Name)
            .HasMaxLength(100);

            ///<summary> Franquicia </summary>
            entity.Property(p => p.Franchise).HasMaxLength(100);
            ///<summary> número </summary>
            entity.Property(p => p.Number).IsRequired();
            ///<summary> Exclusivo valor por defecto false </summary>
            entity.Property(p => p.Exclusive).HasDefaultValue(false);
            ///<summary> Precio requerido </summary>
            entity.Property(p => p.Price).IsRequired(true);
            ///<summary> Eliminado, valor por defecto false </summary>
            entity.Property(p => p.IsDelete).HasDefaultValue(false);
            ///<summary> Fecha de creado </summary>
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            ///<summary> Fecha de actualizado </summary>
            entity.Property(p => p.UpdatedAt).IsRequired(false).HasDefaultValue(null);
            ///<summary> Fecha de eliminado </summary>
            entity.Property(p => p.DeleteAT).IsRequired(false).HasDefaultValue(null);
            ///<summary> Versión de la fila para control de concurrencia </summary>
            entity.Property(s => s.RowVersion).IsRowVersion();

            //<summary> Relación con categoria </summary>
            entity.HasOne(f => f.Category)
                .WithMany(c => c.Funkos)
                .HasForeignKey(f => f.CategoryId);

            entity.HasQueryFilter(f => !f.IsDelete);
            
        });
        
        modelBuilder.Entity<Category>(entity =>
        {
            ///<summary> UUID como id </summary>
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasValueGenerator<YoutubeIdValueGenerator>()
                .ValueGeneratedOnAdd();
            ///<summary> Nombre requerido </summary>
            entity.Property(e => e.Name).IsRequired();
            ///<summary> Control de concurrencia </summary>
            entity.Property(c => c.RowVersion).IsRowVersion();
            entity.Property(c =>c.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            ///<summary> Fecha de actualización valor por defecto "ahora" </summary>
            entity.Property(c =>c.UpdatedAt).IsRequired(false).HasDefaultValue(null);
            ///<summary> Fecha de eliminado </summary>
            entity.Property(c => c.DeleteAT).IsRequired(false).HasDefaultValue(null);
            ///<summary> Versión de la fila para control de concurrencia </summary>
            entity.Property(s => s.RowVersion).IsRowVersion();


            entity.HasData(
                new Category
                {
                    Id = YoutubeIdValueGenerator.GenerateId(),
                    Name = "Anime",
                    Description = "Figuras Funko de series y películas de anime."
                },
                new Category
                {
                    Id = YoutubeIdValueGenerator.GenerateId(),
                    Name = "Marvel",
                    Description = "Figuras Funko de superhéroes y personajes de Marvel."
                },
                new Category
                {
                    Id = "e823IW7x_iC", // para crear desde consulta más rapido
                    Name = "DC Comics",
                    Description = "Figuras Funko de héroes y villanos de DC Comics."
                },
                new Category
                {
                    Id = YoutubeIdValueGenerator.GenerateId(),
                    Name = "Disney",
                    Description = "Figuras Funko de personajes clásicos y modernos de Disney."
                },
                new Category
                {
                    Id = YoutubeIdValueGenerator.GenerateId(),
                    Name = "Videojuegos",
                    Description = "Figuras Funko de personajes del mundo de los videojuegos."
                },
                new Category
                {
                    Id = YoutubeIdValueGenerator.GenerateId(),
                    Name = "Películas y series",
                    Description = "Figuras Funko de películas y series de televisión."
                }
            );
        });
        modelBuilder.Entity<User>(entity =>
        {
            ///<summary> Index como llave </summary>
           entity.HasKey(u => u.Id);
            ///<summary> indice en email ya que se utiliza mucho en consultas </summary>
           entity.HasIndex(u => u.Email);
            ///<summary> Nombre de usuario requerido y con un másximo largo de 255 </summary>
           entity.Property(u => u.Username).IsRequired().HasMaxLength(255);
            ///<summary> Email de usuario requerido y con un másximo largo de 255 </summary>
           entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            ///<summary> Contraseña hasheada de usuario requerido y con un másximo largo de 255 </summary>
           entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
            ///<summary> Contraseña hasheada de usuario requerido y con un másximo largo de 255 </summary>
           entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
            ///<summary> Rol del usuario con un másximo largo de 50 </summary>
           entity.Property(u => u.Role).HasMaxLength(50);
            ///<summary> Eliminado falso por defecto </summary>
           entity.Property(u => u.IsDeleted).HasDefaultValue(false);
            ///<summary> Última conexión null por defecto </summary>
           entity.Property(u => u.LastLoginAt).HasDefaultValue(null);
            ///<summary> Última conexión null por defecto </summary>
           entity.Property(u => u.LastLoginAt).HasDefaultValue(null);

            // Los otros datos no los utilizo de momento
            
            entity.HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "kevin@correo.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                    Role = Role.ADMIN
                }
            );
        });
    }
    
}