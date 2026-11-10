using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.models;
using Moq;
using NUnit.Framework;

[TestFixture]
public class FunkoControllerTests
{
    private Mock<IFunkoService> _service = null!;
    private Mock<IFunkoPriceHistoryService> _historyService = null!;

    private FunkoController _controller = null!;

    private FunkoDto _funko = null!;
    private CategoryDto _category = null!;
    private SupplierDto _supplier = null!;

    private const long FunkoId = 1;

    [SetUp]
    public void SetUp()
    {
        // Arrange
        _service = new Mock<IFunkoService>();
        _historyService = new Mock<IFunkoPriceHistoryService>();

        _controller = new FunkoController(
            _service.Object,
            _historyService.Object
        );

        _category = new CategoryDto(
            "category-1",
            "Marvel",
            "Figuras de Marvel",
            1
        );

        _supplier = new SupplierDto(
            "supplier-1",
            "Proveedor",
            1
        );

        _funko = new FunkoDto(
            FunkoId,
            "Spider-Man",
            "Marvel",
            100,
            _category,
            false,
            20,
            "/uploads/funkos/spiderman.png",
            _supplier,
            1
        );
    }

    // =========================================================
    // CREATE
    // =========================================================

    [Test]
    public async Task CreateFunko_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new FunkoCreateRequestDto(
            "Spider-Man",
            "Marvel",
            100,
            _supplier.Id,
            _category.Id,
            false,
            20
        );

        _service
            .Setup(s => s.CreateFunkoAsync(request))
            .ReturnsAsync(Result.Success<FunkoDto, DomainError>(_funko));

        // Act
        var result = await _controller.CreateFunko(request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();

        var response = result.Result.Should()
            .BeOfType<CreatedAtActionResult>().Subject;

        response.ActionName.Should().Be(nameof(FunkoController.GetFunkoById));
        response.Value.Should().BeEquivalentTo(_funko);
    }

    // =========================================================
    // READ
    // =========================================================

    [Test]
    public async Task GetFunkoById_ReturnsOk()
    {
        // Arrange
        _service
            .Setup(s => s.GetFunkoByIdAsync(FunkoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<FunkoDto, DomainError>(_funko));

        // Act
        var result = await _controller.GetFunkoById(
            FunkoId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();

        var response = result.Result.Should()
            .BeOfType<OkObjectResult>().Subject;

        response.Value.Should().BeEquivalentTo(_funko);
    }

    [Test]
    public async Task GetFunkoById_WhenFunkoDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _service
            .Setup(s => s.GetFunkoByIdAsync(FunkoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<FunkoDto, DomainError>(
                FunkoError.NotFound(FunkoId)));

        // Act
        var result = await _controller.GetFunkoById(
            FunkoId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Test]
    public async Task GetAllFunkos_ReturnsOk()
    {
        // Arrange
        var funkos = new List<FunkoDto> { _funko };

        _service
            .Setup(s => s.GetAllFunkos(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<FunkoDto>, DomainError>(funkos));

        // Act
        var result = await _controller.GetAllFunkos(CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();

        var response = result.Result.Should()
            .BeOfType<OkObjectResult>().Subject;

        response.Value.Should().BeEquivalentTo(funkos);
    }

    // =========================================================
    // PAGINATION
    // =========================================================

    [Test]
    public async Task GetPaginatedFunkos_ReturnsOk()
    {
        // Arrange
        var funkos = new PaginatedResouce<FunkoDto>(
            new List<FunkoDto> { _funko },
            1,
            1,
            false,
            false
        );

        _service
            .Setup(s => s.GetPaginatedFunkos(
                1, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<PaginatedResouce<FunkoDto>, DomainError>(
                funkos));

        // Act
        var result = await _controller.GetPaginatedFunkos(
            1, 5, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();

        var response = result.Result.Should()
            .BeOfType<OkObjectResult>().Subject;

        response.Value.Should().BeEquivalentTo(funkos);
    }

    [Test]
    public async Task GetPaginatedFunkos_WithInvalidPageNumber_ReturnsBadRequest()
    {
        // Arrange
        var pageNumber = 0;
        var pageSize = 5;

        // Act
        var result = await _controller.GetPaginatedFunkos(
            pageNumber, pageSize, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Test]
    public async Task GetPaginatedFunkos_WithInvalidPageSize_ReturnsBadRequest()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 0;

        // Act
        var result = await _controller.GetPaginatedFunkos(
            pageNumber, pageSize, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    [Test]
    public async Task UpdateFunko_ReturnsOk()
    {
        // Arrange
        var request = new FunkoUpdateRequestDto(
            "Spider-Man actualizado",
            "Marvel",
            100,
            false,
            null,
            25,
            1
        );

        _service
            .Setup(s => s.UpdateFunkoAsync(
                FunkoId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<FunkoDto, DomainError>(_funko));

        // Act
        var result = await _controller.UpdateFunko(
            FunkoId, request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task UpdateCategoryFromFunko_ReturnsOk()
    {
        // Arrange
        var request = new FunkoUpdateCategoryRequestDto(
            _category.Id,
            1
        );

        _service
            .Setup(s => s.UpdateCategoryFromAsync(
                FunkoId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<FunkoDto, DomainError>(_funko));

        // Act
        var result = await _controller.UpdateCategoryFromFunko(
            FunkoId, request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task UpdateSupplierFromFunko_ReturnsOk()
    {
        // Arrange
        var request = new FunkoUpdateSupplierDto(
            _supplier.Id,
            1
        );

        _service
            .Setup(s => s.UpdateSupplierFromFunko(
                FunkoId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<FunkoDto, DomainError>(_funko));

        // Act
        var result = await _controller.UpdateSupplierFromFunko(
            FunkoId, request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task UpdatePriceFromFunko_ReturnsOk()
    {
        // Arrange
        var request = new FunkoUpdatePriceRequestDto(25, 1);

        var response = new FunkoPriceUpdatedResponseDto(
            25,
            new PriceHistoryModifiedResponse("Historial actualizado")
        );

        _service
            .Setup(s => s.UpdatePriceAsync(
                FunkoId, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<FunkoPriceUpdatedResponseDto, DomainError>(
                response));

        // Act
        var result = await _controller.UpdatePriceFromFunko(
            FunkoId, request, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // =========================================================
    // DELETE / RESTORE
    // =========================================================

    [Test]
    public async Task DeleteFunko_ReturnsOk()
    {
        // Arrange
        var response = new FunkoDeletedResponseDto(
            "Funko eliminado correctamente",
            new PriceHistoryModifiedResponse("Historial eliminado")
        );

        _service
            .Setup(s => s.DeleteFunkoAsync(
                FunkoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<FunkoDeletedResponseDto, DomainError>(
                response));

        // Act
        var result = await _controller.DeleteFunko(
            FunkoId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task RestoreFunko_ReturnsOk()
    {
        // Arrange
        _service
            .Setup(s => s.RestoreDeletedFunkoAsync(
                FunkoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<FunkoDto, DomainError>(_funko));

        // Act
        var result = await _controller.RestoreFunko(
            FunkoId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // =========================================================
    // UPDATE IMAGE
    // =========================================================

    [Test]
    public async Task UploadFunkoImage_ReturnsOk()
    {
        // Arrange
        var file = new Mock<IFormFile>().Object;

        _service
            .Setup(s => s.UpdateImageFromFunkoAsync(
                FunkoId, file, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<FunkoDto, DomainError>(_funko));

        // Act
        var result = await _controller.UploadFunkoImage(
            FunkoId, file, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    // =========================================================
    // PRICE HISTORY
    // =========================================================

    [Test]
    public async Task GetFunkoPriceHistory_ReturnsOk()
    {
        // Arrange
        var history = new FunkoPrieceHistoryResponsesDto(
            new List<PriceHistoryDto>(),
            30,
            10
        );

        _historyService
            .Setup(s => s.GetHistoryAsync(
                FunkoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<FunkoPrieceHistoryResponsesDto, DomainError>(
                history));

        // Act
        var result = await _controller.GetFunkoPriceHistory(
            FunkoId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task DeleteFunkoPriceHistoryById_ReturnsOk()
    {
        // Arrange
        var response = new PriceHistoryModifiedResponse("Historial eliminado");

        _historyService
            .Setup(s => s.DeleteHistoryAsync(
                FunkoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(response));

        // Act
        var result = await _controller.DeleteFunkoPriceHistoryById(
            FunkoId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Test]
    public async Task RestoreFunkoPriceHistoryById_ReturnsOk()
    {
        // Arrange
        var response = new PriceHistoryModifiedResponse("Historial restaurado");

        _historyService
            .Setup(s => s.RestoreHistoryAsync(
                FunkoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(response));

        // Act
        var result = await _controller.RestoreFunkoPriceHistoryById(
            FunkoId, CancellationToken.None);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }
}