
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;
using Testcontainers.MongoDb;

[TestFixture]
public class SuppliersRepositoryTests
{
    private MongoDbContainer _container = null!;
    private AppDbMongoContext _context = null!;
    private SuppliersRepository _repository = null!;

    /// <summary>
    /// Configuración de la conexión con MongoDB
    /// </summary>
    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _container = new MongoDbBuilder()
            .WithImage("mongo:8")
            .Build();

        await _container.StartAsync();
    }

    [SetUp]
    public async Task Setup()
    {
        // Limpiar base de datos antes de cada test
        var client = new MongoClient(_container.GetConnectionString());
        client.DropDatabase("supplier_test");

        var options = new DbContextOptionsBuilder<AppDbMongoContext>()
            .UseMongoDB(
                _container.GetConnectionString(),
                "supplier_test")
            .Options;

        _context = new AppDbMongoContext(options);

        _repository = new SuppliersRepository(_context);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _context.DisposeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _container.DisposeAsync();
    }

    // Auxiliar para obtener un proveedor válido
    public Supplier CreateSupplierValid()
    {
        return new Supplier
        {
            Id = YoutubeIdValueGenerator.GenerateId(),
            Name = $"Proveedor-{Guid.NewGuid():N}"
        };
    }

    /// <summary>
    /// Crear proveedor
    /// </summary>
    [Test]
    public async Task CreateSupplier_exist_returnSupplier()
    {
        // Arrange
        var supplier = CreateSupplierValid();

        // Act
        var created = await _repository.CreateSupplierAsync(
            new SupplierCreateRequestDto(supplier.Name),
            CancellationToken.None
        );

        // Assert
        Assert.That(created.Id, Is.Not.Null);
        Assert.That(created.Name, Is.EqualTo(supplier.Name));
    }

    /// <summary>
    /// Obtener todos los proveedores
    /// </summary>
    [Test]
    public async Task GetAllSuppliers_exist_returnList()
    {
        // Arrange
        Supplier[] suppliers = new Supplier[5];

        for (int i = 0; i < suppliers.Length; i++)
        {
            suppliers[i] = CreateSupplierValid();
        }

        // Act
        foreach (var supplier in suppliers)
        {
            await _repository.CreateSupplierAsync(
                new SupplierCreateRequestDto(supplier.Name),
                CancellationToken.None
            );
        }

        var response = await _repository.GetAllSuppliersAsync(
            CancellationToken.None
        );

        // Assert
        Assert.That(response, Is.Not.Empty);
        Assert.That(response.Count, Is.EqualTo(5));
    }

    /// <summary>
    /// Obtener todos los proveedores sin resultados
    /// </summary>
    [Test]
    public async Task GetAllSuppliers_NotExist_returnEmptyList()
    {
        // Arrange

        // Act
        var response = await _repository.GetAllSuppliersAsync(
            CancellationToken.None
        );

        // Assert
        Assert.That(response, Is.Empty);
    }

    /// <summary>
    /// Obtener proveedor por ID
    /// </summary>
    [Test]
    public async Task GetSupplierById_exist_returnSupplier()
    {
        // Arrange
        var supplier = CreateSupplierValid();

        // Act
        var created = await _repository.CreateSupplierAsync(
            new SupplierCreateRequestDto(supplier.Name),
            CancellationToken.None
        );

        var response = await _repository.GetSupplierByIdAsync(
            created.Id,
            CancellationToken.None
        );

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Id, Is.EqualTo(created.Id));
        Assert.That(response.Name, Is.EqualTo(supplier.Name));
    }

    /// <summary>
    /// Obtener proveedor inexistente
    /// </summary>
    [Test]
    public async Task GetSupplierById_NotExist_returnNull()
    {
        // Arrange

        // Act
        var response = await _repository.GetSupplierByIdAsync(
            "507f1f77bcf86cd799439011",
            CancellationToken.None
        );

        // Assert
        Assert.That(response, Is.Null);
    }

    /// <summary>
    /// Comprobar si existe un proveedor por nombre
    /// </summary>
    [Test]
    public async Task ExistByName_exist_returnTrue()
    {
        // Arrange
        var supplier = CreateSupplierValid();

        // Act
        await _repository.CreateSupplierAsync(
            new SupplierCreateRequestDto(supplier.Name),
            CancellationToken.None
        );

        var response = await _repository.ExistByNameAsync(
            supplier.Name,
            CancellationToken.None
        );

        // Assert
        Assert.That(response, Is.True);
    }

    /// <summary>
    /// Comprobar si existe un proveedor inexistente
    /// </summary>
    [Test]
    public async Task ExistByName_NotExist_returnFalse()
    {
        // Arrange

        // Act
        var response = await _repository.ExistByNameAsync(
            "Proveedor inexistente",
            CancellationToken.None
        );

        // Assert
        Assert.That(response, Is.False);
    }

    /// <summary>
    /// Comprobar existencia de proveedor eliminado
    /// </summary>
    [Test]
    public async Task ExistByName_Deleted_returnTrue()
    {
        // Arrange
        var supplier = CreateSupplierValid();

        var created = await _repository.CreateSupplierAsync(
            new SupplierCreateRequestDto(supplier.Name),
            CancellationToken.None
        );

        // Act
        await _repository.DeleteSupplierAsync(
            created.Id,
            CancellationToken.None
        );

        var response = await _repository.ExistByNameAsync(
            created.Name,
            CancellationToken.None
        );

        // Assert
        Assert.That(response, Is.True);
    }

    /// <summary>
    /// Actualizar proveedor
    /// </summary>
    [Test]
    public async Task UpdateSupplier_exist_returnUpdatedSupplier()
    {
        // Arrange
        var supplier = CreateSupplierValid();

        var created = await _repository.CreateSupplierAsync(
            new SupplierCreateRequestDto(supplier.Name),
            CancellationToken.None
        );

        // Act
        var updated = await _repository.UpdateSupplierAsync(
            created.Id,
            new SupplierUpdateRequestDto(
                "Proveedor actualizado",
                created.RowVersion
            ),
            CancellationToken.None
        );

        // Assert
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Id, Is.EqualTo(created.Id));
        Assert.That(updated.Name, Is.EqualTo("Proveedor actualizado"));
        Assert.That(updated.Name, Is.Not.EqualTo(supplier.Name));
    }
    /// <summary>
    /// Actualizar proveedor con error de concurrencia
    /// </summary>
    [Test]
    public async Task UpdateSupplier_alredyUpdated_DbUpdateConcurrencyException()
    {
        // Arrange
        var supplier = CreateSupplierValid();

        // Act
        var created = await _repository.CreateSupplierAsync(
            new SupplierCreateRequestDto(supplier.Name),
            CancellationToken.None
        );

        Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            await _repository.UpdateSupplierAsync(
                created.Id,
                new SupplierUpdateRequestDto("Proveedor actualizado",created.RowVersion - 1 // Version anterior
                ),
                CancellationToken.None
            )
        );

        // Assert
    }

    /// <summary>
    /// Actualizar proveedor inexistente
    /// </summary>
   [Test]
    public async Task UpdateSupplier_NotExist_returnNull()
    {
        // Arrange
        var supplier = CreateSupplierValid();

        // Act
        var response = await _repository.UpdateSupplierAsync(
            "507f1f77bcf86cd799439011",
            new SupplierUpdateRequestDto(supplier.Name, 0),
            CancellationToken.None
        );

        // Assert
        Assert.That(response, Is.Null);
    }
    /// <summary>
    /// Eliminar proveedor
    /// </summary>
    [Test]
    public async Task DeleteSupplier_exist_returnTrue()
    {
        // Arrange
        var supplier = CreateSupplierValid();

        var created = await _repository.CreateSupplierAsync(
            new SupplierCreateRequestDto(supplier.Name),
            CancellationToken.None
        );

        // Act
        var deleted = await _repository.DeleteSupplierAsync(
            created.Id,
            CancellationToken.None
        );

        var response = await _repository.GetSupplierByIdAsync(
            created.Id,
            CancellationToken.None
        );

        // Assert
        Assert.That(deleted, Is.True);
        Assert.That(response, Is.Null);
    }

    /// <summary>
    /// Eliminar proveedor inexistente
    /// </summary>
    [Test]
    public async Task DeleteSupplier_NotExist_returnFalse()
    {
        // Arrange

        // Act
        var response = await _repository.DeleteSupplierAsync(
            "507f1f77bcf86cd799439011",
            CancellationToken.None
        );

        // Assert
        Assert.That(response, Is.False);
    }

    /// <summary>
    /// Restaurar proveedor eliminado
    /// </summary>
    [Test]
    public async Task RestoreSupplier_exist_returnTrue()
    {
        // Arrange
        var supplier = CreateSupplierValid();

        var created = await _repository.CreateSupplierAsync(
            new SupplierCreateRequestDto(supplier.Name),
            CancellationToken.None
        );

        await _repository.DeleteSupplierAsync(
            created.Id,
            CancellationToken.None
        );

        // Act
        var restored = await _repository.RestoreSupplierAsync(
            created.Id,
            CancellationToken.None
        );

        var response = await _repository.GetSupplierByIdAsync(
            created.Id,
            CancellationToken.None
        );

        // Assert
        Assert.That(restored, Is.True);
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.IsDeleted, Is.False);
        Assert.That(response.DeleteAt, Is.Null);
    }

    /// <summary>
    /// Restaurar proveedor inexistente
    /// </summary>
    [Test]
    public async Task RestoreSupplier_NotExist_returnFalse()
    {
        // Arrange

        // Act
        var response = await _repository.RestoreSupplierAsync(
            "507f1f77bcf86cd799439011",
            CancellationToken.None
        );

        // Assert
        Assert.That(response, Is.False);
    }

    /// <summary>
    /// Obtener proveedores paginados
    /// </summary>
    [Test]
    public async Task GetPaginatedSuppliers_returnList()
    {
        // Arrange
        Supplier[] suppliers = new Supplier[5];

        for (int i = 0; i < suppliers.Length; i++)
        {
            suppliers[i] = CreateSupplierValid();
        }

        // Act
        foreach (var supplier in suppliers)
        {
            await _repository.CreateSupplierAsync(
                new SupplierCreateRequestDto(supplier.Name),
                CancellationToken.None
            );
        }

        var response = await _repository.GetPaginatedSuppliersAsync(
            1,
            5,
            CancellationToken.None
        );

        // Assert
        Assert.That(response.items, Is.Not.Empty);
        Assert.That(response.count, Is.EqualTo(5));
        Assert.That(response.totalPages, Is.EqualTo(1));
        Assert.That(response.HasPreviousPage, Is.False);
        Assert.That(response.HasNextPage, Is.False);
    }

    /// <summary>
    /// Obtener página intermedia
    /// </summary>
    [Test]
    public async Task GetPaginatedSuppliers_IntermediatePage_returnList()
    {
        // Arrange
        Supplier[] suppliers = new Supplier[5];

        for (int i = 0; i < suppliers.Length; i++)
        {
            suppliers[i] = CreateSupplierValid();
        }

        // Act
        foreach (var supplier in suppliers)
        {
            await _repository.CreateSupplierAsync(
                new SupplierCreateRequestDto(supplier.Name),
                CancellationToken.None
            );
        }

        var response = await _repository.GetPaginatedSuppliersAsync(
            2,
            2,
            CancellationToken.None
        );

        // Assert
        Assert.That(response.items.Count, Is.EqualTo(2));
        Assert.That(response.count, Is.EqualTo(5));
        Assert.That(response.totalPages, Is.EqualTo(3));
        Assert.That(response.HasPreviousPage, Is.True);
        Assert.That(response.HasNextPage, Is.True);
    }

    /// <summary>
    /// Obtener última página
    /// </summary>
    [Test]
    public async Task GetPaginatedSuppliers_LastPage_returnList()
    {
        // Arrange
        Supplier[] suppliers = new Supplier[5];

        for (int i = 0; i < suppliers.Length; i++)
        {
            suppliers[i] = CreateSupplierValid();
        }

        // Act
        foreach (var supplier in suppliers)
        {
            await _repository.CreateSupplierAsync(
                new SupplierCreateRequestDto(supplier.Name),
                CancellationToken.None
            );
        }

        var response = await _repository.GetPaginatedSuppliersAsync(
            3,
            2,
            CancellationToken.None
        );

        // Assert
        Assert.That(response.items.Count, Is.EqualTo(1));
        Assert.That(response.count, Is.EqualTo(5));
        Assert.That(response.totalPages, Is.EqualTo(3));
        Assert.That(response.HasPreviousPage, Is.True);
        Assert.That(response.HasNextPage, Is.False);
    }

    /// <summary>
    /// Obtener paginación sin resultados
    /// </summary>
    [Test]
    public async Task GetPaginatedSuppliers_NotExist_returnEmptyList()
    {
        // Arrange

        // Act
        var response = await _repository.GetPaginatedSuppliersAsync(
            1,
            5,
            CancellationToken.None
        );

        // Assert
        Assert.That(response.items, Is.Empty);
        Assert.That(response.count, Is.EqualTo(0));
        Assert.That(response.totalPages, Is.EqualTo(0));
        Assert.That(response.HasPreviousPage, Is.False);
        Assert.That(response.HasNextPage, Is.False);
    }
}
