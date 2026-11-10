using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

[TestFixture]
public class SupplierControllerTests
{
    private Mock<ISupplierService> _service = null!;
    private SupplierController _controller = null!;

    private SupplierDto _supplier = null!;

    private const string SupplierId = "supplier-1";

    [SetUp]
    public void SetUp()
    {
        // Arrange
        _service = new Mock<ISupplierService>();
        _controller = new SupplierController(_service.Object);

        _supplier = new SupplierDto(
            SupplierId,
            "Proveedor",
            1
        );
    }

    // =========================================================
    // CREATE
    // =========================================================

    [Test]
    public async Task CreateSupplier_ReturnsCreatedAtAction()
    {
        // Arrange
        SupplierCreateRequestDto request = null!;

        _service
            .Setup(s => s.CreateSupplierAsync(
                It.IsAny<SupplierCreateRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<SupplierDto, DomainError>(_supplier));

        // Act
        var result = await _controller.CreateSupplier(
            request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();

        var response = result.Should()
            .BeOfType<CreatedAtActionResult>().Subject;

        response.ActionName.Should().Be(nameof(SupplierController.GetSupplierById));
        response.Value.Should().BeEquivalentTo(_supplier);
    }

    [Test]
    public async Task CreateSupplier_WhenServiceFails_ReturnsBadRequest()
    {
        // Arrange
        SupplierCreateRequestDto request = null!;

        _service
            .Setup(s => s.CreateSupplierAsync(
                It.IsAny<SupplierCreateRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<SupplierDto, DomainError>(
                NotFoundError.FromResource("Supplier", SupplierId)));

        // Act
        var result = await _controller.CreateSupplier(
            request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // =========================================================
    // READ
    // =========================================================

    [Test]
    public async Task GetAllSuppliers_ReturnsOk()
    {
        // Arrange
        var suppliers = new List<SupplierDto> { _supplier };

        _service
            .Setup(s => s.GetAllSuppliersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<SupplierDto>, DomainError>(suppliers));

        // Act
        var result = await _controller.GetAllSuppliers(CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var response = result.Should()
            .BeOfType<OkObjectResult>().Subject;

        response.Value.Should().BeEquivalentTo(suppliers);
    }

    [Test]
    public async Task GetAllSuppliers_WhenServiceFails_ReturnsBadRequest()
    {
        // Arrange
        _service
            .Setup(s => s.GetAllSuppliersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<List<SupplierDto>, DomainError>(
                NotFoundError.FromResource("Supplier", SupplierId)));

        // Act
        var result = await _controller.GetAllSuppliers(CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // =========================================================
    // PAGINATION
    // =========================================================

    [Test]
    public async Task GetPaginatedSuppliers_ReturnsOk()
    {
        // Arrange
        var suppliers = new PaginatedResouce<SupplierDto>(
            new List<SupplierDto> { _supplier },
            1,
            1,
            false,
            false
        );

        _service
            .Setup(s => s.GetPaginatedSuppliersAsync(
                1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<PaginatedResouce<SupplierDto>, DomainError>(
                suppliers));

        // Act
        var result = await _controller.GetPaginatedSuppliers(
            1, 10, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var response = result.Should()
            .BeOfType<OkObjectResult>().Subject;

        response.Value.Should().BeEquivalentTo(suppliers);
    }

    [Test]
    public async Task GetPaginatedSuppliers_WithInvalidPageNumber_ReturnsBadRequest()
    {
        // Arrange
        var pageNumber = 0;
        var pageSize = 10;

        // Act
        var result = await _controller.GetPaginatedSuppliers(
            pageNumber, pageSize, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task GetPaginatedSuppliers_WithInvalidPageSize_ReturnsBadRequest()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 0;

        // Act
        var result = await _controller.GetPaginatedSuppliers(
            pageNumber, pageSize, CancellationToken.None);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // =========================================================
    // GET BY ID
    // =========================================================

    [Test]
    public async Task GetSupplierById_ReturnsOk()
    {
        // Arrange
        _service
            .Setup(s => s.GetSupplierByIdAsync(
                SupplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<SupplierDto, DomainError>(_supplier));

        // Act
        var result = await _controller.GetSupplierById(
            SupplierId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var response = result.Should()
            .BeOfType<OkObjectResult>().Subject;

        response.Value.Should().BeEquivalentTo(_supplier);
    }

    [Test]
    public async Task GetSupplierById_WhenSupplierDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _service
            .Setup(s => s.GetSupplierByIdAsync(
                SupplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<SupplierDto, DomainError>(
                NotFoundError.FromResource("Supplier", SupplierId)));

        // Act
        var result = await _controller.GetSupplierById(
            SupplierId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    [Test]
    public async Task UpdateSupplier_ReturnsOk()
    {
        // Arrange
        SupplierUpdateRequestDto request = null!;

        _service
            .Setup(s => s.UpdateSupplierAsync(
                SupplierId,
                It.IsAny<SupplierUpdateRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<SupplierDto, DomainError>(_supplier));

        // Act
        var result = await _controller.UpdateSupplier(
            SupplierId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var response = result.Should()
            .BeOfType<OkObjectResult>().Subject;

        response.Value.Should().BeEquivalentTo(_supplier);
    }

    [Test]
    public async Task UpdateSupplier_WhenSupplierDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        SupplierUpdateRequestDto request = null!;

        _service
            .Setup(s => s.UpdateSupplierAsync(
                SupplierId,
                It.IsAny<SupplierUpdateRequestDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<SupplierDto, DomainError>(
                NotFoundError.FromResource("Supplier", SupplierId)));

        // Act
        var result = await _controller.UpdateSupplier(
            SupplierId, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // =========================================================
    // DELETE
    // =========================================================

    [Test]
    public async Task DeleteSupplier_ReturnsNoContent()
    {
        // Arrange
        _service
            .Setup(s => s.DeleteSupplierAsync(
                SupplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<bool, DomainError>(true));

        // Act
        var result = await _controller.DeleteSupplier(
            SupplierId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task DeleteSupplier_WhenSupplierDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _service
            .Setup(s => s.DeleteSupplierAsync(
                SupplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<bool, DomainError>(
                NotFoundError.FromResource("Supplier", SupplierId)));

        // Act
        var result = await _controller.DeleteSupplier(
            SupplierId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    // =========================================================
    // RESTORE
    // =========================================================

    [Test]
    public async Task RestoreSupplier_ReturnsNoContent()
    {
        // Arrange
        _service
            .Setup(s => s.RestoreSupplierAsync(
                SupplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<bool, DomainError>(true));

        // Act
        var result = await _controller.RestoreSupplier(
            SupplierId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task RestoreSupplier_WhenSupplierDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _service
            .Setup(s => s.RestoreSupplierAsync(
                SupplierId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<bool, DomainError>(
                NotFoundError.FromResource("Supplier", SupplierId)));

        // Act
        var result = await _controller.RestoreSupplier(
            SupplierId, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}