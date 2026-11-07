
using Microsoft.Extensions.Logging;
using Moq;

[TestFixture]
public class CategoryServiceTests
{
    private Mock<ICategoryRepository> _repository = null!;
    private Mock<ILogger<CategoryService>> _logger = null!;
    private Mock<ICache> _cache = null!;
    private CategoryService _service = null!;

    [SetUp]
    public void Setup()
    {
        _repository = new Mock<ICategoryRepository>();
        _logger = new Mock<ILogger<CategoryService>>();
        _cache = new Mock<ICache>();

        _service = new CategoryService(
            _repository.Object,
            _logger.Object,
            _cache.Object
        );
    }

    public Category CreateCategoryValid() => new Category
    {
        Id = YoutubeIdValueGenerator.GenerateId(),
        Name = "DC Comics",
        Description = "Figuras de DC Comics"
    };

    // =========================================================
    // CREATE
    // =========================================================

    [Test]
    public async Task CreateCategory_ValidCategory_Success()
    {
        // Arrange
        var category = CreateCategoryValid();

        var request = new CategoryCreateRequestDto(
            category.Name,
            category.Description
        );

        _repository
            .Setup(r => r.ExistByName(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repository
            .Setup(r => r.CreateCategoryAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var response = await _service.CreateCategoryAsync(request);

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value.Name, Is.EqualTo(category.Name));
        Assert.That(response.Value.Id, Is.EqualTo(category.Id));
    }

    [Test]
    public async Task CreateCategory_ExistingName_Failure()
    {
        // Arrange
        var request = new CategoryCreateRequestDto(
            "DC Comics",
            "Figuras de DC Comics"
        );

        _repository
            .Setup(r => r.ExistByName(request.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var response = await _service.CreateCategoryAsync(request);

        // Assert
        Assert.That(response.IsFailure, Is.True);

        _repository.Verify(
            r => r.CreateCategoryAsync(
                It.IsAny<CategoryCreateRequestDto>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
    }

    // =========================================================
    // GET ALL
    // =========================================================

    [Test]
    public async Task GetAllCategories_ExistingCategories_Success()
    {
        // Arrange
        var categories = new List<Category>
        {
            CreateCategoryValid()
        };

        _repository
            .Setup(r => r.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var response = await _service.GetAllCategoriesAsync();

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value, Has.Count.EqualTo(1));
        Assert.That(response.Value[0].Name, Is.EqualTo(categories[0].Name));
    }

    [Test]
    public async Task GetAllCategories_NoCategories_EmptyList()
    {
        // Arrange
        _repository
            .Setup(r => r.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Category>());

        // Act
        var response = await _service.GetAllCategoriesAsync();

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value, Is.Empty);
    }

    // =========================================================
    // GET BY ID
    // =========================================================

    [Test]
    public async Task GetCategoryById_ExistingCategory_Success()
    {
        // Arrange
        var category = CreateCategoryValid();

        _repository
            .Setup(r => r.GetCategoryById(
                category.Id,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(category);

        // Act
        var response = await _service.GetCategoryByIdAsync(
            category.Id,
            CancellationToken.None
        );

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value!.Id, Is.EqualTo(category.Id));
    }

    [Test]
    public async Task GetCategoryById_NonExistingCategory_Failure()
    {
        // Arrange
        var id = "categoria_inexistente";

        _repository
            .Setup(r => r.GetCategoryById(
                id,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((Category?)null);

        // Act
        var response = await _service.GetCategoryByIdAsync(
            id,
            CancellationToken.None
        );

        // Assert
        Assert.That(response.IsFailure, Is.True);
    }

    // =========================================================
    // GET BY NAME
    // =========================================================

    [Test]
    public async Task GetCategoryByName_ExistingCategory_Success()
    {
        // Arrange
        var category = CreateCategoryValid();

        _repository
            .Setup(r => r.GetCategoryByName(
                category.Name,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(category);

        // Act
        var response = await _service.GetCategoryByNameAsync(
            category.Name,
            CancellationToken.None
        );

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value!.Name, Is.EqualTo(category.Name));
    }

    [Test]
    public async Task GetCategoryByName_NonExistingCategory_Failure()
    {
        // Arrange
        _repository
            .Setup(r => r.GetCategoryByName(
                "Inexistente",
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((Category?)null);

        // Act
        var response = await _service.GetCategoryByNameAsync(
            "Inexistente",
            CancellationToken.None
        );

        // Assert
        Assert.That(response.IsFailure, Is.True);
    }

    // =========================================================
    // PAGINATION
    // =========================================================

    [Test]
    public async Task GetPaginatedCategories_FirstPage_Success()
    {
        // Arrange
        var categories = new List<Category>
        {
            CreateCategoryValid()
        };

        _repository
            .Setup(r => r.GetPaginatedCategoriesAsync(
                1, 10, It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((categories, 1, 1, false, false));

        // Act
        var response = await _service.GetPaginatedCategoriesAsync(1, 10);

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value.Items, Has.Count.EqualTo(1));
        Assert.That(response.Value.Count, Is.EqualTo(1));

        _cache.Verify(
            c => c.SetAsync(
                "CATEGORY:ALL-PAGINATED:1",
                It.IsAny<PaginatedResouce<CategoryDto>>()
            ),
            Times.Once
        );
    }

    [Test]
    public async Task GetPaginatedCategories_SecondPage_Success()
    {
        // Arrange
        var categories = new List<Category>
        {
            CreateCategoryValid()
        };

        _repository
            .Setup(r => r.GetPaginatedCategoriesAsync(
                2, 10, It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((categories, 11, 2, true, false));

        // Act
        var response = await _service.GetPaginatedCategoriesAsync(2, 10);

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value.Items, Has.Count.EqualTo(1));
        Assert.That(response.Value.HasPreviusPage, Is.True);

        _cache.Verify(
            c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<PaginatedResouce<CategoryDto>>()
            ),
            Times.Never
        );
    }

    [Test]
    public async Task GetPaginatedCategories_EmptyPage_DoesNotCache()
    {
        // Arrange
        _repository
            .Setup(r => r.GetPaginatedCategoriesAsync(
                1, 10, It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((
                new List<Category>(),
                0,
                0,
                false,
                false
            ));

        // Act
        var response = await _service.GetPaginatedCategoriesAsync(1, 10);

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value.Count, Is.EqualTo(0));

        _cache.Verify(
            c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<PaginatedResouce<CategoryDto>>()
            ),
            Times.Never
        );
    }

    // =========================================================
    // UPDATE
    // =========================================================

    [Test]
    public async Task UpdateCategory_ExistingCategory_Success()
    {
        // Arrange
        var category = CreateCategoryValid();

        var request = new CategoryUpdateRequestDto(
            "Marvel",
            "Figuras de Marvel",
            1
        );

        category.Name = request.Name;
        category.Description = request.Description;

        _repository
            .Setup(r => r.UpdateCategoryAsync(
                category.Id,
                request,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(category);

        // Act
        var response = await _service.UpdateCategoryAsync(
            category.Id,
            request
        );

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value.Name, Is.EqualTo("Marvel"));

        _cache.Verify(
            c => c.RemoveAsync($"CATEGORY:{category.Id}"),
            Times.Once
        );

        _cache.Verify(
            c => c.RemoveAsync("CATEGORY:ALL-PAGINATED:1"),
            Times.Once
        );
    }

    [Test]
    public async Task UpdateCategory_NonExistingCategory_Failure()
    {
        // Arrange
        var id = "inexistente";

        var request = new CategoryUpdateRequestDto(
            "Marvel",
            "Figuras de Marvel",
            1
        );

        _repository
            .Setup(r => r.UpdateCategoryAsync(
                id,
                request,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((Category?)null);

        // Act
        var response = await _service.UpdateCategoryAsync(id, request);

        // Assert
        Assert.That(response.IsFailure, Is.True);

        _cache.Verify(
            c => c.RemoveAsync(It.IsAny<string>()),
            Times.Never
        );
    }

    // =========================================================
    // DELETE
    // =========================================================

    [Test]
    public async Task DeleteCategory_ExistingCategory_Success()
    {
        // Arrange
        var category = CreateCategoryValid();

        _repository
            .Setup(r => r.DeleteCategoryAsync(
                category.Id,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(true);

        // Act
        var response = await _service.DeleteCategoryAsync(category.Id);

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value, Is.True);

        _cache.Verify(
            c => c.RemoveAsync($"CATEGORY:{category.Id}"),
            Times.Once
        );

        _cache.Verify(
            c => c.RemoveAsync("CATEGORY:ALL-PAGINATED:1"),
            Times.Once
        );
    }

    [Test]
    public async Task DeleteCategory_NonExistingCategory_Failure()
    {
        // Arrange
        _repository
            .Setup(r => r.DeleteCategoryAsync(
                "inexistente",
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(false);

        // Act
        var response = await _service.DeleteCategoryAsync("inexistente");

        // Assert
        Assert.That(response.IsFailure, Is.True);

        _cache.Verify(
            c => c.RemoveAsync(It.IsAny<string>()),
            Times.Never
        );
    }

    // =========================================================
    // RESTORE
    // =========================================================

    [Test]
    public async Task RestoreCategory_DeletedCategory_Success()
    {
        // Arrange
        var category = CreateCategoryValid();

        _repository
            .Setup(r => r.RestoreCategoryAsync(
                category.Id,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(true);

        // Act
        var response = await _service.RestoreCategoryAsync(category.Id);

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value, Is.True);
    }

    [Test]
    public async Task RestoreCategory_NonExistingCategory_Failure()
    {
        // Arrange
        _repository
            .Setup(r => r.RestoreCategoryAsync(
                "inexistente",
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(false);

        // Act
        var response = await _service.RestoreCategoryAsync("inexistente");

        // Assert
        Assert.That(response.IsFailure, Is.True);
    }
}
