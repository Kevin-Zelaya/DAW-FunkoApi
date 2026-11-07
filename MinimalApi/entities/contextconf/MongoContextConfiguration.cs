using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.EntityFrameworkCore.Extensions;
using Serilog;

public abstract class MongoContextConfiguration
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        Log.Debug("[DBCONTEXT-CONFIG] Configurando entidades para el proveedor 'PosgreSql'");
        modelBuilder.Entity<Supplier>(entity =>
        {
            ///<summary> Proveedores como collección </summary>
            entity.ToCollection("suppliers");
            ///<summary> llave id  </summary>
            entity.HasKey(s => s.Id);
            ///<summary> nombre de la clave </summary>
            entity.Property(s => s.Id)
                .HasElementName("_id")
                .ValueGeneratedOnAdd();
            ///<summary> Nombre es requerido </summary>
            entity.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(100);
            ///<summary> Nombre como indice </summary>
            entity.HasIndex(s => s.Name).IsUnique();
            ///<summary> Eliminado por defecto en false </summary>
            entity.Property(s => s.IsDeleted)
                .HasDefaultValue(false);
            ///<summary> Fecha de creación </summary>
            entity.Property(s => s.CreateAt)
                .IsRequired();
            ///<summary> fecha de actualización inicializa en null </summary>
            entity.Property(s => s.UpdateAt)
                .IsRequired(false);
            ///<summary> fecha de eliminación inicializa en null </summary>
            entity.Property(s => s.DeleteAt)
                .IsRequired(false);
            ///<summary> Conrol de concurrencia </summary>
            entity.Property(s => s.RowVersion)
                .IsRowVersion(); // cuando se agreguen el resto de metodos
            ///<summary> Filtro para no mostrar eliminado </summary>
            entity.HasQueryFilter(s => !s.IsDeleted);
            
            entity.HasData(
                new Supplier { Id = "6aee3ce4035ea0d4b4a44432", Name = "Funko Europe", IsDeleted = false },
                new Supplier { Id = ObjectId.GenerateNewId().ToString(), Name = "Collector Store", IsDeleted = false },
                new Supplier { Id = ObjectId.GenerateNewId().ToString(), Name = "Pop In A Box", IsDeleted = false },
                new Supplier { Id = ObjectId.GenerateNewId().ToString(), Name = "Entertainment Earth", IsDeleted = false },
                new Supplier { Id = ObjectId.GenerateNewId().ToString(), Name = "BigBadToyStore", IsDeleted = false },
                new Supplier { Id = ObjectId.GenerateNewId().ToString(), Name = "Amazon Collectibles", IsDeleted = false },
                new Supplier { Id = ObjectId.GenerateNewId().ToString(), Name = "Geek Store", IsDeleted = false },
                new Supplier { Id = ObjectId.GenerateNewId().ToString(), Name = "Comic World", IsDeleted = false },
                new Supplier { Id = ObjectId.GenerateNewId().ToString(), Name = "Pop Culture Shop", IsDeleted = false },
                new Supplier { Id = ObjectId.GenerateNewId().ToString(), Name = "Collector's Corner", IsDeleted = false }
            );
        });
    }
    
}