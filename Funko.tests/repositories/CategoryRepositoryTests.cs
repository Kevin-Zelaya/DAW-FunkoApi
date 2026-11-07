using Microsoft.EntityFrameworkCore;
using Moq;
using Testcontainers.PostgreSql;

[TestFixture]
public class CategoryRepositoryTests
{
    private PostgreSqlContainer _container = null!;
    private AppDbContext _context = null!;
    private CategoryRepository _repository = null!;

    /// <summary>
    /// Configuración de la conexión
    /// con la base de datos
    /// </summary>
    /// <returns></returns>
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
        // inyectamos el contexto
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
    // Auxiliar para obtener una categoria valida
    public Category CreateCategoryValid()
    {
        return new Category
                {
                    Id = YoutubeIdValueGenerator.GenerateId(), // para crear desde consulta más rapido
                    Name = "DC Comics",
                    Description = "Figuras Funko de héroes y villanos de DC Comics."
                };
    }
    /// <summary>
    /// Crear categoría
    /// </summary>
    [Test]
    public async Task CreateCategoy_exist_returnCategory()
    {
        // Arrange
        var category = CreateCategoryValid();

        // Arc
        var exist = await _repository.ExistByName(category.Name); // comprobación de si existe
        var created = await _repository.CreateCategoryAsync(new CategoryCreateRequestDto
        (
            category.Name,
            category.Description
        ));
        // Assert
        Assert.That(category.Id, Is.Not.Null); // id asignado
        Assert.That(category.Name, Is.EqualTo(created.Name));
        Assert.That(category.Description , Is.EqualTo(created.Description));
        Assert.That(exist , Is.True);
    }
    /// <summary>
    /// Buscar una categoria creada
    /// </summary>
    [Test]
    public async Task GetCategoryById_exist_returnCategory()
    {
        // Arrange
        var category = CreateCategoryValid();

        // Arc
        var created = await _repository.CreateCategoryAsync(new CategoryCreateRequestDto
        (
            category.Name,
            category.Description
        ));
        var response = await _repository.GetCategoryById(created.Id);
        // Assert
        Assert.That(response!.Id, Is.EqualTo(created.Id)); // id asignado
        Assert.That(category.Name, Is.EqualTo(response.Name));
        Assert.That(category.Description , Is.EqualTo(response.Description));
    }
    /// <summary>
    /// Buscar una categoria creada por su nombre
    /// </summary>
    [Test]
    public async Task GetCategoryByName_exist_returnCategory()
    {
        // Arrange
        var category = CreateCategoryValid();

        // Arc
        var created = await _repository.CreateCategoryAsync(new CategoryCreateRequestDto
        (
            "Josemanuel",
            category.Description
        ));
        var response = await _repository.GetCategoryByName(created.Name);
        // Assert
        Assert.That(response!.Id, Is.EqualTo(created.Id)); // id asignado
        Assert.That(created.Name, Is.EqualTo(response.Name));
        Assert.That(category.Description , Is.EqualTo(response.Description));
    }
    /// <summary>
    /// Buscar una categoria no existente
    /// </summary>
    [Test]
    public async Task GetCategoryById_Notexist_returnNull()
    {
        // Arrange
        var category = CreateCategoryValid();

        // Arc
        var response = await _repository.GetCategoryById("e823Ie7x_iC");
        // Assert
        Assert.That(response, Is.Null); // null
        
    }
    /// <summary>
    /// actualizar categoría
    /// </summary>
    [Test]
    public async Task UpdateCategory_exist_UpdatedCategory()
    {
        // Arrange
        var category = CreateCategoryValid();

        // Arc
        var created = await _repository.CreateCategoryAsync(new CategoryCreateRequestDto
        (
            category.Name,
            category.Description
        ));
        var updated = await _repository.UpdateCategoryAsync(
            created.Id,
            new CategoryUpdateRequestDto("Prueba", "Para probar", created.RowVersion)
        );
        // Assert
        Assert.That(updated!.Id, Is.EqualTo(created.Id)); // id asignado
        Assert.That(category.Name, Is.Not.EqualTo(updated.Name));
        Assert.That(category.Description , Is.Not.EqualTo(updated.Description));
    }
    /// <summary>
    /// actualizar categoría error de concurrencia
    /// </summary>
    [Test]
    public async Task UpdateCategory_alredyUpdated_DbUpdateConcurrencyException()
    {
        // Arrange
        var category = CreateCategoryValid();

        // Arc
        var created = await _repository.CreateCategoryAsync(new CategoryCreateRequestDto
        (
            category.Name,
            category.Description
        ));
        Assert.ThrowsAsync<DbUpdateConcurrencyException>( async () =>
             await _repository.UpdateCategoryAsync( 
                        created.Id,
                        new CategoryUpdateRequestDto("Prueba", "Para probar", created.RowVersion-1) // Version anterior
                    )
        );
        // Assert
    }
    /// <summary>
    /// Eliminar categoria
    /// </summary>
    [Test]
    public async Task DeleteCategory_exist_returntru()
    {
        // Arrange
        var category = CreateCategoryValid();

        // Arc
        var created = await _repository.CreateCategoryAsync(new CategoryCreateRequestDto
        (
            category.Name,
            category.Description
        ));
        var deleted = await _repository.DeleteCategoryAsync(
            created.Id
        );
        // Assert
        Assert.That(deleted!, Is.True); // id asignado
    }
    /// <summary>
    /// Restaurar categoria eliminada
    /// </summary>
    [Test]
    public async Task RestoreCategory_exist_returntru()
    {
        // Arrange
        var category = CreateCategoryValid();

        // Arc
        var created = await _repository.CreateCategoryAsync(new CategoryCreateRequestDto
        (
            category.Name,
            category.Description
        ));
        var restore = await _repository.RestoreCategoryAsync( // se puede restaurar aunque no este eliminado ya que es eliminado lógico
            created.Id
        );
        // Assert
        Assert.That(restore!, Is.True); // id asignado
    }
    /// <summary>
    /// Restaurar categoria eliminada
    /// </summary>
    [Test]
    public async Task GetAllCtegories_returnlist()
    {
        // Arrange
        Category[] categories = new Category[5];
        for (int i = 0; i < categories.Length; i++)
        {
            categories[i] = CreateCategoryValid();
        }

        // Arc
        foreach(var cat in categories)
        {    
            await _repository.CreateCategoryAsync(new CategoryCreateRequestDto
            (
                cat.Name,
                cat.Description
            ));
        }
        var response = await _repository.GetAllCategoriesAsync();
        // Assert
        Assert.That(response, Is.Not.Empty); // id asignado
    }
    /// <summary>
    /// Obtener lista páginada de categorias
    /// </summary>
    [Test]
    public async Task GetPáginateCategory_returnlist()
    {
        // Arrange
        Category[] categories = new Category[5];
        for (int i = 0; i < categories.Length; i++)
        {
            categories[i] = CreateCategoryValid();
        }

        // Arc
        foreach(var cat in categories)
        {    
            await _repository.CreateCategoryAsync(new CategoryCreateRequestDto
            (
                cat.Name,
                cat.Description
            ));
        }
        var response = await _repository.GetPaginatedCategoriesAsync(1, 5);
        // Assert
        Assert.That(response.items, Is.Not.Empty); 
        Assert.That(response.count, Is.EqualTo(5)); 
        Assert.That(response.totalPages, Is.EqualTo(1)); 
        Assert.That(response.HasPreviousPage, Is.False); 
        Assert.That(response.HasNextPage, Is.False); 
    }


}