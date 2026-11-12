
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using MongoDB.Bson;

[TestFixture]
public class FunkoRepositoryTests
{
    private PostgreSqlContainer _container = null!;
    private AppDbContext _context = null!;
    private FunkoRepository _repository = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithDatabase("funko_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        await _container.StartAsync();
    }

    [SetUp]
    public async Task Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        _context = new AppDbContext(options);

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

    // =========================================================
    // HELPERS
    // =========================================================

    public Funko CreateFunkoValid()
    {
        return new Funko
        {
            Name = $"Batman_{Guid.NewGuid().ToString()[..8]}",
            Franchise = "DC Comics",
            Number = 100,
            SupplierId = ObjectId.GenerateNewId().ToString(),
            CategoryId = null,
            Exclusive = false,
            Price = 15.99,
            Imagen = "batman.jpg",
            IsDelete = false,
            CreatedAt = DateTime.UtcNow,
            RowVersion = 1
        };
    }

    public Category CreateCategoryValid()
    {
        return new Category
        {
            Id = YoutubeIdValueGenerator.GenerateId(),
            Name = "DC Comics",
            Description = "Figuras de DC Comics",
            IsDelete = false,
            CreatedAt = DateTime.UtcNow,
            RowVersion = 1
        };
    }

    public FunkoUpdateRequestDto CreateFunkoUpdateValid(uint rowVersion)
    {
        return new FunkoUpdateRequestDto(
            Name: "Batman actualizado",
            Franchise: "DC Comics",
            Number: 200,
            Exclusive: true,
            Imagen: "batman-updated.jpg",
            RowVersion: rowVersion
        );
    }

    // =========================================================
    // CREATE
    // =========================================================

    [Test]
    public async Task CreateFunkoAsync_ValidFunko_CreatesFunko()
    {
        // Arrange
        var funko = CreateFunkoValid();

        // Act
        var result = await _repository.CreateFunkoAsync(funko);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.GreaterThan(0));
        Assert.That(result.Name, Is.EqualTo(funko.Name));
    }

    // =========================================================
    // READ
    // =========================================================

    [Test]
    public async Task GetFunkoByIdAsync_ExistingFunko_ReturnsFunko()
    {
        // Arrange
        var funko = CreateFunkoValid();
        _context.Funkos.Add(funko);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetFunkoByIdAsync(funko.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(funko.Id));
        Assert.That(result.Name, Is.EqualTo(funko.Name));
    }

    [Test]
    public async Task GetFunkoByIdAsync_NonExistingFunko_ReturnsNull()
    {
        // Arrange
        // Act
        var result = await _repository.GetFunkoByIdAsync(999);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetAllFunkosAsync_ExistingFunkos_ReturnsAllFunkos()
    {
        // Arrange
        var funko1 = CreateFunkoValid();
        var funko2 = CreateFunkoValid();
        funko2.Name = "Superman";

        _context.Funkos.AddRange(funko1, funko2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllFunkosAsync();

        // Assert
        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
    public async Task GetAllFunkosAsync_NoFunkos_ReturnsEmptyList()
    {
        // Arrange
        // Act
        var result = await _repository.GetAllFunkosAsync();

        // Assert
        Assert.That(result, Is.Not.Empty);
    }

    // =========================================================
    // PAGINATION
    // =========================================================

    [Test]
    public async Task GetPaginatedFunkosAsync_FirstPage_ReturnsCorrectPagination()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            var funko = CreateFunkoValid();
            funko.Name = $"Funko {i}";
            _context.Funkos.Add(funko);
        }

        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPaginatedFunkosAsync(1, 2);

        // Assert
        Assert.That(result.items, Has.Count.EqualTo(2));
        Assert.That(result.count, Is.EqualTo(6));
        Assert.That(result.totalPages, Is.EqualTo(3));
        Assert.That(result.HasPreviousPage, Is.False);
        Assert.That(result.HasNextPage, Is.True);
    }

    [Test]
    public async Task GetPaginatedFunkosAsync_LastPage_ReturnsCorrectPagination()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            var funko = CreateFunkoValid();
            funko.Name = $"Funko {i}";
            _context.Funkos.Add(funko);
        }

        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetPaginatedFunkosAsync(3, 2);

        // Assert
        Assert.That(result.items, Has.Count.EqualTo(2));
        Assert.That(result.count, Is.EqualTo(6));
        Assert.That(result.totalPages, Is.EqualTo(3));
        Assert.That(result.HasPreviousPage, Is.True);
        Assert.That(result.HasNextPage, Is.False);
    }

    // =========================================================
    // EXIST
    // =========================================================

    [Test]
    public async Task ExistByNameAsync_ExistingFunko_ReturnsTrue()
    {
        // Arrange
        var funko = CreateFunkoValid();
        _context.Funkos.Add(funko);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistByNameAsync(funko.Name, 0);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task ExistByNameAsync_NonExistingFunko_ReturnsFalse()
    {
        // Arrange
        // Act
        var result = await _repository.ExistByNameAsync("No existe", 0);

        // Assert
        Assert.That(result, Is.False);
    }

    // =========================================================
    // UPDATE
    // =========================================================

    [Test]
    public async Task UpdateFunkoAsync_ExistingFunko_UpdatesFunko()
    {
        // Arrange
        var funko = CreateFunkoValid();
        _context.Funkos.Add(funko);
        await _context.SaveChangesAsync();

        var rowVersion = (await _context.Funkos
            .AsNoTracking()
            .FirstAsync(f => f.Id == funko.Id)).RowVersion;

        var request = CreateFunkoUpdateValid(rowVersion);

        // Act
        var result = await _repository.UpdateFunkoAsync(funko.Id, request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Name, Is.EqualTo(request.Name));
        Assert.That(result.Franchise, Is.EqualTo(request.Franchise));
        Assert.That(result.Number, Is.EqualTo(request.Number));
        Assert.That(result.Exclusive, Is.EqualTo(request.Exclusive));
    }

    [Test]
    public async Task UpdateFunkoAsync_NonExistingFunko_ReturnsNull()
    {
        // Arrange
        var request = CreateFunkoUpdateValid(1);

        // Act
        var result = await _repository.UpdateFunkoAsync(999, request);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateCategoryFromFunkoAsync_ExistingFunko_UpdatesCategory()
    {
        // Arrange
        var category = CreateCategoryValid();
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var funko = CreateFunkoValid();
        _context.Funkos.Add(funko);
        await _context.SaveChangesAsync();

        var rowVersion = (await _context.Funkos
            .AsNoTracking()
            .FirstAsync(f => f.Id == funko.Id)).RowVersion;

        var request = new FunkoUpdateCategoryRequestDto(
            category.Id!,
            rowVersion
        );

        // Act
        var result = await _repository.UpdateCategoryFromFunkoAsync(
            funko.Id, request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.CategoryId, Is.EqualTo(category.Id));
    }

    [Test]
    public async Task UpdateCategoryFromFunkoAsync_NonExistingFunko_ReturnsNull()
    {
        // Arrange
        var category = CreateCategoryValid();
        var request = new FunkoUpdateCategoryRequestDto(
            category.Id!,
            1
        );

        // Act
        var result = await _repository.UpdateCategoryFromFunkoAsync(999, request);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateSuplierFromFunkoAsync_ExistingFunko_UpdatesSupplier()
    {
        // Arrange
        var funko = CreateFunkoValid();
        _context.Funkos.Add(funko);
        await _context.SaveChangesAsync();

        var rowVersion = (await _context.Funkos
            .AsNoTracking()
            .FirstAsync(f => f.Id == funko.Id)).RowVersion;

        var supplierId = ObjectId.GenerateNewId().ToString();

        var request = new FunkoUpdateSupplierDto(
            supplierId,
            rowVersion
        );

        // Act
        var result = await _repository.UpdateSuplierFromFunkoAsync(
            funko.Id, request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.SupplierId, Is.EqualTo(supplierId));
    }

    [Test]
    public async Task UpdateSuplierFromFunkoAsync_NonExistingFunko_ReturnsNull()
    {
        // Arrange
        var request = new FunkoUpdateSupplierDto(
            ObjectId.GenerateNewId().ToString(),
            1
        );

        // Act
        var result = await _repository.UpdateSuplierFromFunkoAsync(999, request);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateImageFromFunkoAsync_ExistingFunko_UpdatesImage()
    {
        // Arrange
        var funko = CreateFunkoValid();
        _context.Funkos.Add(funko);
        await _context.SaveChangesAsync();

        var image = "new-image.jpg";

        // Act
        var result = await _repository.UpdateImageFromFunkoAsync(
            funko.Id, image);

        // Assert
        Assert.That(result, Is.True);

        var updated = await _context.Funkos
            .AsNoTracking()
            .FirstAsync(f => f.Id == funko.Id);

        Assert.That(updated.Imagen, Is.EqualTo(image));
    }

    [Test]
    public async Task UpdateImageFromFunkoAsync_NonExistingFunko_ReturnsFalse()
    {
        // Arrange
        // Act
        var result = await _repository.UpdateImageFromFunkoAsync(
            999, "image.jpg");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task UpdatePriceFromFunkoAsync_ExistingFunko_UpdatesPrice()
    {
        // Arrange
        var funko = CreateFunkoValid();
        _context.Funkos.Add(funko);
        await _context.SaveChangesAsync();

        var rowVersion = (await _context.Funkos
            .AsNoTracking()
            .FirstAsync(f => f.Id == funko.Id)).RowVersion;

        var request = new FunkoUpdatePriceRequestDto(25.99, rowVersion);

        // Act
        var result = await _repository.UpdatePriceFromFunkoAsync(
            funko.Id, request);

        // Assert
        Assert.That(result, Is.True);

        var updated = await _context.Funkos
            .AsNoTracking()
            .FirstAsync(f => f.Id == funko.Id);

        Assert.That(updated.Price, Is.EqualTo(request.Price));
    }

    [Test]
    public async Task UpdatePriceFromFunkoAsync_NonExistingFunko_ReturnsFalse()
    {
        // Arrange
        var request = new FunkoUpdatePriceRequestDto(25.99, 1);

        // Act
        var result = await _repository.UpdatePriceFromFunkoAsync(999, request);

        // Assert
        Assert.That(result, Is.False);
    }

    // =========================================================
    // DELETE / RESTORE
    // =========================================================

    [Test]
    public async Task DeleteFunkoAsync_ExistingFunko_DeletesFunko()
    {
        // Arrange
        var funko = CreateFunkoValid();
        _context.Funkos.Add(funko);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteFunkoAsync(funko.Id);

        // Assert
        Assert.That(result, Is.True);

        var deleted = await _context.Funkos
            .IgnoreQueryFilters()
            .FirstAsync(f => f.Id == funko.Id);

        Assert.That(deleted.IsDelete, Is.True);
        Assert.That(deleted.DeleteAT, Is.Not.Null);
    }

    [Test]
    public async Task DeleteFunkoAsync_NonExistingFunko_ReturnsFalse()
    {
        // Arrange
        // Act
        var result = await _repository.DeleteFunkoAsync(999);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task RestoreFunkoAsync_DeletedFunko_RestoresFunko()
    {
        // Arrange
        var funko = CreateFunkoValid();
        funko.IsDelete = true;
        funko.DeleteAT = DateTime.UtcNow;

        _context.Funkos.Add(funko);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.RestoreFunkoAsync(funko.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.IsDelete, Is.False);
        Assert.That(result.DeleteAT, Is.Null);
    }

    [Test]
    public async Task RestoreFunkoAsync_NonDeletedFunko_ReturnsNull()
    {
        // Arrange
        var funko = CreateFunkoValid();
        _context.Funkos.Add(funko);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.RestoreFunkoAsync(funko.Id);

        // Assert
        Assert.That(result, Is.Null);
    }
}
