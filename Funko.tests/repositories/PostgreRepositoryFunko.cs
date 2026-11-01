using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Testcontainers.PostgreSql;

[TestFixture]
public class PosgreRepositoryTests
{
    private PostgreSqlContainer _container = null!;

    private AppDbContext _context = null!;
    private FunkoRepository _repository = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithDatabase("funkos_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        await _container.StartAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        _context = new AppDbContext(options);

        // Cada test empieza con una base de datos limpia.
        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        _repository = new FunkoRepository(_context);
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

    private static Funko CrearFunkoValido()
    {
        return new Funko
        {
            Name = "Spider-Man",
            Franchise = "Marvel",
            Number = 100,
            Exclusive = false,
            Price = (double)15.99m,
            Category = new Category
            {
                Name = "Superheroes",
                Description = "Funko de superheroes"
            },
            Supplier = CrearSupplierValido(),
            RowVersion = 954
        };
    }
    private static FunkoUpdateCategoryRequestDto CrearCategoryUpdateRequestValido(uint row)
    {
        return new FunkoUpdateCategoryRequestDto
        (
            SupplierId : Guid.Parse("10000000-0000-0000-0000-000000000000"),
            RowVersion : row
        );
    }
    private static Supplier CrearSupplierValido()
    {
        return new Supplier
        {
            Name = "Proveedor de prueba"
        };
    }

    [Test]
    public async Task GetAllFunkosAsync_SinDatos_RetornaListaVacia()
    {
        // Arrange

        // Act
        var resultado = await _repository.GetAllFunkosAsync();

        // Assert
        Assert.That(resultado, Is.Empty);
    }

    [Test]
    public async Task GetFunkoByIdAsync_NoExiste_RetornaNull()
    {
        // Arrange
        int id = 999;

        // Act
        var resultado = await _repository.GetFunkoByIdAsync(id);

        // Assert
        Assert.That(resultado, Is.Null);
    }

    [Test]
    public async Task DeleteFunkoAsync_NoExiste_RetornaFalse()
    {
        // Arrange
        int id = 999;

        // Act
        var resultado = await _repository.DeleteFunkoAsync(id);

        // Assert
        Assert.That(resultado, Is.False);
    }

    [Test]
    public async Task RestoreFunkoAsync_NoExiste_RetornaNull()
    {
        // Arrange
        int id = 999;

        // Act
        var resultado = await _repository.RestoreFunkoAsync(id);

        // Assert
        Assert.That(resultado, Is.Null);
    }

    [Test]
    public async Task ExistSupplierByIdAsync_NoExiste_RetornaFalse()
    {
        // Arrange
        string id = "hu8555awe";

        // Act
        var resultado = await _repository.ExistSupplierByIdAsync(id);

        // Assert
        Assert.That(resultado, Is.False);
    }

    [Test]
    public async Task GetPaginatedFunkosAsync_SinDatos_RetornaResultadoVacio()
    {
        // Arrange
        int pageNumber = 1;
        int pageSize = 5;

        // Act
        var resultado = await _repository.GetPaginatedFunkosAsync(
            pageNumber,
            pageSize
        );

        // Assert
        Assert.That(resultado.items, Is.Empty);
        Assert.That(resultado.count, Is.EqualTo(0));
        Assert.That(resultado.totalPages, Is.EqualTo(0));
        Assert.That(resultado.HasPreviousPage, Is.False);
        Assert.That(resultado.HasNextPage, Is.False);
    }
    [Test]
    public async Task UpdateFunkoAsync_NoExiste_RetornaNull()
    {
        // Arrange
        int id = 999;

        // Act
        var resultado = await _repository.UpdateFunkoAsync(
            id,
            null!
        );

        // Assert
        Assert.That(resultado, Is.Null);
    }

    [Test]
    public async Task UpdateCategoryFromFunkoAsync_NoExiste_RetornaNull()
    {
        // Arrange
        int id = 999;

        // Act
        var resultado = await _repository.UpdateCategoryFromFunkoAsync(
            id,
            null!
        );

        // Assert
        Assert.That(resultado, Is.Null);
    }

    [Test]
    public async Task UpdateSuplierFromFunkoAsync_NoExiste_RetornaNull()
    {
        // Arrange
        int id = 999;
        var supplier = new FunkoUpdateSupplierDto
        (
            SupplierId : "dsfko65es",
            RowVersion : 954
        );

        // Act
        var resultado = await _repository.UpdateSuplierFromFunkoAsync(
            id,
            supplier
        );

        // Assert
        Assert.That(resultado, Is.Null);
    }
    [Test]
    public async Task GetAllCategoriesAsync_SinFunkos_RetornaListaVacia()
    {
        // Arrange

        // Act
        var resultado = await _repository.GetAllCategoriesAsync();

        // Assert
        Assert.That(resultado, Is.Empty);
    }

    [Test]
    public async Task CreateFunkoAsync_FunkoValido_GuardaElFunko()
    {
        // Arrange
        var funko = CrearFunkoValido();

        // Act
        var resultado = await _repository.CreateFunkoAsync(funko);

        // Assert
        Assert.That(resultado.Id, Is.GreaterThan(0));

        var guardado = await _context.Funkos
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == resultado.Id);

        Assert.That(guardado, Is.Not.Null);
        Assert.That(guardado!.Name, Is.EqualTo(funko.Name));
    }

    [Test]
    public async Task GetFunkoByIdAsync_Existe_RetornaFunko()
    {
        // Arrange
        var funko = CrearFunkoValido();

        _context.Funkos.Add(funko);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.GetFunkoByIdAsync(funko.Id);

        // Assert
        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.Id, Is.EqualTo(funko.Id));
        Assert.That(resultado.Name, Is.EqualTo(funko.Name));
    }

    [Test]
    public async Task UpdateFunkoAsync_Existe_ActualizaLosDatos()
    {
        // Arrange
        var funko = CrearFunkoValido();

        _context.Funkos.Add(funko);
        await _context.SaveChangesAsync();

        var request = CrearFunkoUpdateRequestValido(funko.RowVersion);

        // Act
        var resultado = await _repository.UpdateFunkoAsync(
            funko.Id,
            request
        );

        // Assert
        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.Name, Is.EqualTo(request.Name));
        Assert.That(resultado.Franchise, Is.EqualTo(request.Franchise));
        Assert.That(resultado.Number, Is.EqualTo(request.Number));
        Assert.That(resultado.Exclusive, Is.EqualTo(request.Exclusive));
        Assert.That(resultado.Price, Is.EqualTo(request.Price));
    }
    private static FunkoUpdateRequestDto CrearFunkoUpdateRequestValido(uint row)
    {
        return new FunkoUpdateRequestDto
        (
        
            Name : "Spider-Man actualizado",
            Franchise : "Marvel",
            Number : 200,
            Exclusive : true,
            Price : (double)19.99m,
            RowVersion : row
        );
    }
    [Test]
    public async Task UpdateCategoryFromFunkoAsync_Existe_ActualizaCategoria()
    {
        // Arrange
        var funko = CrearFunkoValido();

        _context.Funkos.Add(funko);
        await _context.SaveChangesAsync();

        var request = CrearCategoryUpdateRequestValido(funko.RowVersion);
        var save = new Category
        {
            Id = request.SupplierId,
            Name = "prueba",
            Description = "prueba"
        };
        _context.Categories.Add(save);

        // Act
        var resultado = await _repository.UpdateCategoryFromFunkoAsync(
            funko.Id,
            request
        );

        // Assert
        Assert.That(resultado, Is.Not.Null);
    }

    [Test]
    public async Task UpdateSuplierFromFunkoAsync_Existe_CambiaProveedor()
    {
        // Arrange
        var supplier = CrearSupplierValido();
        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();
        
        var funko = CrearFunkoValido();

        _context.Funkos.Add(funko);
        await _context.SaveChangesAsync();
    var updateSupplier = new FunkoUpdateSupplierDto(
                SupplierId: supplier.Id,
                RowVersion: funko.RowVersion
            );

        // Act
        var resultado = await _repository.UpdateSuplierFromFunkoAsync(
            funko.Id,
            updateSupplier
        );

        // Assert
        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.SupplierId, Is.EqualTo(supplier.Id));
        Assert.That(resultado.Supplier, Is.Not.Null);
    }

    [Test]
    public async Task DeleteFunkoAsync_Existe_MarcaComoEliminado()
    {
        // Arrange
        var funko = CrearFunkoValido();

        _context.Funkos.Add(funko);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.DeleteFunkoAsync(funko.Id);

        // Assert
        Assert.That(resultado, Is.True);

        var eliminado = await _context.Funkos
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id == funko.Id);

        Assert.That(eliminado, Is.Not.Null);
        Assert.That(eliminado!.IsDelete, Is.True);
        Assert.That(eliminado.DeleteAT, Is.Not.Null);
    }

    [Test]
    public async Task DeleteFunkoAsync_Existe_NoApareceEnConsultasNormales()
    {
        // Arrange
        var funko = CrearFunkoValido();

        _context.Funkos.Add(funko);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteFunkoAsync(funko.Id);

        var resultado = await _repository.GetFunkoByIdAsync(funko.Id);

        // Assert
        Assert.That(resultado, Is.Null);
    }

    [Test]
    public async Task RestoreFunkoAsync_EstaEliminado_RestauraFunko()
    {
        // Arrange
        var funko = CrearFunkoValido();

        _context.Funkos.Add(funko);
        await _context.SaveChangesAsync();

        await _repository.DeleteFunkoAsync(funko.Id);

        // Act
        var resultado = await _repository.RestoreFunkoAsync(funko.Id);

        // Assert
        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado!.IsDelete, Is.False);
        Assert.That(resultado.DeleteAT, Is.Null);

        var recuperado = await _repository.GetFunkoByIdAsync(funko.Id);

        Assert.That(recuperado, Is.Not.Null);
    }

    [Test]
    public async Task ExistSupplierByIdAsync_Existe_RetornaTrue()
    {
        // Arrange
        var supplier = CrearSupplierValido();

        _context.Suppliers.Add(supplier);
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _repository.ExistSupplierByIdAsync(supplier.Id);

        // Assert
        Assert.That(resultado, Is.True);
    }
}