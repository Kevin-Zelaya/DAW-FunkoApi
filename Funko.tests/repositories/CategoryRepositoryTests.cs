using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

[TestFixture]
[NonParallelizable]
public class CategoryRepositoryTests
{
    private PostgreSqlContainer _container = null!;
    private AppDbContext _context = null!;
    private CategoryRepository _repository = null!;

    // =========================================================
    // CONFIGURACIÓN
    // =========================================================

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithDatabase("category_test")
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

        _repository = new CategoryRepository(_context);
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
    // MÉTODOS AUXILIARES
    // =========================================================

    private Category CreateCategoryValid(string? name = null)
    {
        return new Category
        {
            Id = Guid.NewGuid().ToString(),
            Name = name ?? $"Categoría-{Guid.NewGuid()}",
            Description = "Figuras Funko de prueba."
        };
    }

    private async Task<Category> InsertCategory(string? name = null)
    {
        var category = CreateCategoryValid(name);

        return await _repository.CreateCategoryAsync(
            new CategoryCreateRequestDto(
                category.Name,
                category.Description
            ));
    }

    // =========================================================
    // CREATE
    // =========================================================

    [Test]
    public async Task CreateCategory_Valid_ReturnCategory()
    {
        // Arrange
        var category = CreateCategoryValid();

        // Act
        var exist = await _repository.ExistByName(category.Name);

        var created = await _repository.CreateCategoryAsync(
            new CategoryCreateRequestDto(
                category.Name,
                category.Description
            ));

        // Assert
        Assert.That(exist, Is.False);
        Assert.That(created.Id, Is.Not.Null.And.Not.Empty);
        Assert.That(created.Name, Is.EqualTo(category.Name));
        Assert.That(created.Description, Is.EqualTo(category.Description));
    }

    // =========================================================
    // GET BY ID
    // =========================================================

    [Test]
    public async Task GetCategoryById_Exist_ReturnCategory()
    {
        // Arrange
        var created = await InsertCategory();

        // Act
        var response = await _repository.GetCategoryById(created.Id);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Id, Is.EqualTo(created.Id));
        Assert.That(response.Name, Is.EqualTo(created.Name));
        Assert.That(response.Description, Is.EqualTo(created.Description));
    }

    [Test]
    public async Task GetCategoryById_NotExist_ReturnNull()
    {
        // Act
        var response = await _repository.GetCategoryById("id-inexistente");

        // Assert
        Assert.That(response, Is.Null);
    }

    // =========================================================
    // GET BY NAME
    // =========================================================

    [Test]
    public async Task GetCategoryByName_Exist_ReturnCategory()
    {
        // Arrange
        var created = await InsertCategory("Josemanuel");

        // Act
        var response = await _repository.GetCategoryByName(created.Name);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Id, Is.EqualTo(created.Id));
        Assert.That(response.Name, Is.EqualTo(created.Name));
        Assert.That(response.Description, Is.EqualTo(created.Description));
    }

    // =========================================================
    // UPDATE
    // =========================================================

    [Test]
    public async Task UpdateCategory_Exist_ReturnUpdatedCategory()
    {
        // Arrange
        var created = await InsertCategory();

        // Act
        var updated = await _repository.UpdateCategoryAsync(
            created.Id,
            new CategoryUpdateRequestDto(
                "Prueba",
                "Para probar",
                created.RowVersion
            ));

        // Assert
        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Id, Is.EqualTo(created.Id));
        Assert.That(updated.Name, Is.EqualTo("Prueba"));
        Assert.That(updated.Description, Is.EqualTo("Para probar"));
    }

    [Test]
    public async Task UpdateCategory_WrongVersion_ThrowsConcurrencyException()
    {
        // Arrange
        var created = await InsertCategory();

        var wrongVersion = created.RowVersion == 0
            ? 1u
            : created.RowVersion - 1;

        // Act y Assert
        Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            await _repository.UpdateCategoryAsync(
                created.Id,
                new CategoryUpdateRequestDto(
                    "Prueba",
                    "Para probar",
                    wrongVersion
                )
            ));
    }

    // =========================================================
    // DELETE
    // =========================================================

    [Test]
    public async Task DeleteCategory_Exist_ReturnTrue()
    {
        // Arrange
        var created = await InsertCategory();

        // Act
        var deleted = await _repository.DeleteCategoryAsync(created.Id);

        // Assert
        Assert.That(deleted, Is.True);
    }

    // =========================================================
    // RESTORE
    // =========================================================

    [Test]
    public async Task RestoreCategory_Deleted_ReturnTrue()
    {
        // Arrange
        var created = await InsertCategory();

        await _repository.DeleteCategoryAsync(created.Id);

        // Act
        var restored = await _repository.RestoreCategoryAsync(created.Id);

        // Assert
        Assert.That(restored, Is.True);
    }

    // =========================================================
    // GET ALL
    // =========================================================

    [Test]
    public async Task GetAllCategories_ReturnList()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            await InsertCategory($"Categoría-{i}");
        }

        // Act
        var response = await _repository.GetAllCategoriesAsync();

        // Assert
        Assert.That(response, Is.Not.Empty);
    }

    // =========================================================
    // PAGINATION
    // =========================================================

    [Test]
    public async Task GetPaginateCategory_ReturnList()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            await InsertCategory($"Categoría-{i}");
        }

        // Act
        var response = await _repository.GetPaginatedCategoriesAsync(1, 5);

        // Assert
        Assert.That(response.items, Is.Not.Empty);
        Assert.That(response.count, Is.EqualTo(5));
        Assert.That(response.totalPages, Is.EqualTo(1));
        Assert.That(response.HasPreviousPage, Is.False);
        Assert.That(response.HasNextPage, Is.False);
    }
}
