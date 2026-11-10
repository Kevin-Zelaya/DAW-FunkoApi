using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

[TestFixture]
public class CategoryControllerTests
{
    private Mock<ICategoryService> _service = null!;
    private CategoryController _controller = null!;

    private CategoryDto _category = null!;

    private const string CategoryId = "category-1";

    [SetUp]
    public void SetUp()
    {
        // Arrange
        _service = new Mock<ICategoryService>();
        _controller = new CategoryController(_service.Object);

        _category = new CategoryDto(
            CategoryId,
            "Marvel",
            "Figuras de Marvel",
            1
        );
    }

    // =========================================================
    // GET ALL
    // =========================================================

    [Test]
    public async Task GetAllCategories_ReturnsOk()
    {
        // Arrange
        var categories = new List<CategoryDto> { _category };

        _service
            .Setup(s => s.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<CategoryDto>, DomainError>(categories));

        // Act
        var result = await _controller.GetAllCategories(CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();

        var response = result.Result.Should()
            .BeOfType<OkObjectResult>().Subject;

        response.Value.Should().BeEquivalentTo(categories);
    }

    // =========================================================
    // GET PAGINATED
    // =========================================================

    [Test]
    public async Task GetPaginatedCategories_ReturnsOk()
    {
        // Arrange
        var categories = new PaginatedResouce<CategoryDto>(
            new List<CategoryDto> { _category },
            1,
            1,
            false,
            false
        );

        _service
            .Setup(s => s.GetPaginatedCategoriesAsync(
                1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<PaginatedResouce<CategoryDto>, DomainError>(
                categories));

        // Act
        var result = await _controller.GetPaginatedCategories(
            1, 10, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();

        var response = result.Result.Should()
            .BeOfType<OkObjectResult>().Subject;

        response.Value.Should().BeEquivalentTo(categories);
    }

    [Test]
    public async Task GetPaginatedCategories_WithInvalidPageNumber_ReturnsBadRequest()
    {
        // Arrange
        var pageNumber = 0;
        var pageSize = 10;

        // Act
        var result = await _controller.GetPaginatedCategories(
            pageNumber, pageSize, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task GetPaginatedCategories_WithInvalidPageSize_ReturnsBadRequest()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 0;

        // Act
        var result = await _controller.GetPaginatedCategories(
            pageNumber, pageSize, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // =========================================================
    // GET BY ID
    // =========================================================

    [Test]
    public async Task GetCategoryById_ReturnsOk()
    {
        // Arrange
        _service
            .Setup(s => s.GetCategoryByIdAsync(
                CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<CategoryDto, DomainError>(_category));

        // Act
        var result = await _controller.GetCategoryById(
            CategoryId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();

        var response = result.Result.Should()
            .BeOfType<OkObjectResult>().Subject;

        response.Value.Should().BeEquivalentTo(_category);
    }

    [Test]
    public async Task GetCategoryById_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _service
            .Setup(s => s.GetCategoryByIdAsync(
                CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<CategoryDto, DomainError>(
                NotFoundError.FromResource("Category", CategoryId)));

        // Act
        var result = await _controller.GetCategoryById(
            CategoryId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    // =========================================================
    // CREATE
    // =========================================================

    [Test]
    public async Task CreateCategory_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CategoryCreateRequestDto(
            "Marvel",
            "Figuras de Marvel"
        );

        _service
            .Setup(s => s.CreateCategoryAsync(
                request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<CategoryDto, DomainError>(_category));

        // Act
        var result = await _controller.CreateCategory(
            request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();

        var response = result.Result.Should()
            .BeOfType<CreatedAtActionResult>().Subject;

        response.ActionName.Should().Be(nameof(CategoryController.GetCategoryById));
        response.Value.Should().BeEquivalentTo(_category);
    }

    // =========================================================
    // UPDATE
    // =========================================================

    [Test]
    public async Task UpdateCategory_ReturnsOk()
    {
        // Arrange
        var request = new CategoryUpdateRequestDto(
            "Marvel",
            "Figuras actualizadas de Marvel",
            1
        );

        _service
            .Setup(s => s.UpdateCategoryAsync(
                CategoryId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<CategoryDto, DomainError>(_category));

        // Act
        var result = await _controller.UpdateCategory(
            CategoryId, request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();

        var response = result.Result.Should()
            .BeOfType<OkObjectResult>().Subject;

        response.Value.Should().BeEquivalentTo(_category);
    }

    // =========================================================
    // DELETE
    // =========================================================

    [Test]
    public async Task DeleteCategory_ReturnsNoContent()
    {
        // Arrange
        _service
            .Setup(s => s.DeleteCategoryAsync(
                CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<bool, DomainError>(true));

        // Act
        var result = await _controller.DeleteCategory(
            CategoryId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NoContentResult>();
    }

    // =========================================================
    // RESTORE
    // =========================================================

    [Test]
    public async Task RestoreCategory_ReturnsNoContent()
    {
        // Arrange
        _service
            .Setup(s => s.RestoreCategoryAsync(
                CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<bool, DomainError>(true));

        // Act
        var result = await _controller.RestoreCategory(
            CategoryId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NoContentResult>();
    }
}