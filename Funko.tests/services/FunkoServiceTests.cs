using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using CSharpFunctionalExtensions;
using MinimalApi.Services.Email;

[TestFixture]
public class FunkoServiceTests
{
    private Mock<IFunkoRepository> _repository = null!;
    private Mock<ISupplierService> _supplierService = null!;
    private Mock<ICategoryService> _categoryService = null!;
    private Mock<IStorageService> _storageService = null!;
    private Mock<ILogger<FunkoService>> _logger = null!;
    private Mock<ICache> _cache = null!;
    private Mock<IFunkoPriceHistoryService> _historyService = null!;
    private Mock<IEmailQueueService> _emailQueue = null!;
    private Mock<INotificationService> _notificationService = null!;

    private FunkoService _service = null!;

    private Funko _funko = null!;
    private SupplierDto _supplier = null!;
    private CategoryDto _category = null!;

    [SetUp]
    public void SetUp()
    {
        // Arrange
        _repository = new Mock<IFunkoRepository>();
        _supplierService = new Mock<ISupplierService>();
        _categoryService = new Mock<ICategoryService>();
        _storageService = new Mock<IStorageService>();
        _logger = new Mock<ILogger<FunkoService>>();
        _cache = new Mock<ICache>();
        _historyService = new Mock<IFunkoPriceHistoryService>();
        _emailQueue = new Mock<IEmailQueueService>();
        _notificationService = new Mock<INotificationService>();

        _service = new FunkoService(
            _repository.Object,
            _supplierService.Object,
            _categoryService.Object,
            _storageService.Object,
            _logger.Object,
            _cache.Object,
            _historyService.Object,
            _emailQueue.Object,
            _notificationService.Object
        );

        _supplier = new SupplierDto("supplier-1", "Proveedor", 1);

        _category = new CategoryDto(
            "category-1",
            "Categoría",
            "Descripción",
            1
        );

        _funko = new Funko
        {
            Id = 1,
            Name = "Spider-Man",
            Franchise = "Marvel",
            Number = 100,
            SupplierId = _supplier.Id,
            CategoryId = _category.Id,
            Category = _category.ToEntity(),
            Exclusive = false,
            Price = 20,
            Imagen = "/uploads/funkos/default.png",
            RowVersion = 1
        };
    }

    // =========================================================
    // CREATE
    // =========================================================

    [Test]
    public async Task CreateFunkoAsync_ReturnsCreatedFunko()
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

        _repository
            .Setup(x => x.ExistByNameAsync(request.Name, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _supplierService
            .Setup(x => x.GetSupplierByIdAsync(_supplier.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<SupplierDto, DomainError>(_supplier));

        _categoryService
            .Setup(x => x.GetCategoryByIdAsync(_category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<CategoryDto, DomainError>(_category));

        _repository
            .Setup(x => x.CreateFunkoAsync(It.IsAny<Funko>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_funko);

        _historyService
            .Setup(x => x.AddHistoryAsync(
                _funko.Id,
                (decimal)_funko.Price,
                0,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<PriceHistoryModifiedResponse, DomainError>(
                new PriceHistoryModifiedResponse("Historial añadido")));

        // Act
        var result = await _service.CreateFunkoAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Name, Is.EqualTo(_funko.Name));
    }

    // =========================================================
    // READ
    // =========================================================

    [Test]
    public async Task GetFunkoByIdAsync_ReturnsFunko()
    {
        // Arrange
        _cache
            .Setup(x => x.GetTAsync<FunkoDto>(It.IsAny<string>()))
            .ReturnsAsync((FunkoDto?)null);

        _repository
            .Setup(x => x.GetFunkoByIdAsync(_funko.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_funko);

        _supplierService
            .Setup(x => x.GetSupplierByIdAsync(_supplier.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<SupplierDto, DomainError>(_supplier));

        // Act
        var result = await _service.GetFunkoByIdAsync(_funko.Id, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(_funko.Id));
    }

    [Test]
    public async Task GetAllFunkos_ReturnsAllFunkos()
    {
        // Arrange
        _repository
            .Setup(x => x.GetAllFunkosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Funko> { _funko });

        _supplierService
            .Setup(x => x.GetAllSuppliersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<SupplierDto>, DomainError>(
                new List<SupplierDto> { _supplier }));

        // Act
        var result = await _service.GetAllFunkos();

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Has.Count.EqualTo(1));
        Assert.That(result.Value[0].Name, Is.EqualTo(_funko.Name));
    }

    [Test]
    public async Task GetPaginatedFunkos_ReturnsPaginatedFunkos()
    {
        // Arrange
        _cache
            .Setup(x => x.GetTAsync<PaginatedResouce<FunkoDto>>(It.IsAny<string>()))
            .ReturnsAsync((PaginatedResouce<FunkoDto>?)null);

        _repository
            .Setup(x => x.GetPaginatedFunkosAsync(1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((
                new List<Funko> { _funko },
                1,
                1,
                false,
                false
            ));

        _supplierService
            .Setup(x => x.GetAllSuppliersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<List<SupplierDto>, DomainError>(
                new List<SupplierDto> { _supplier }));

        // Act
        var result = await _service.GetPaginatedFunkos(1, 10);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Items, Has.Count.EqualTo(1));
        Assert.That(result.Value.Count, Is.EqualTo(1));
    }

    // =========================================================
    // UPDATE
    // =========================================================

    [Test]
    public async Task UpdateFunkoAsync_ReturnsUpdatedFunko()
    {
        // Arrange
        var request = new FunkoUpdateRequestDto(
            "Spider-Man actualizado",
            "Marvel",
            100,
            false,
            null,
            1
        );

        var updatedFunko = new Funko
        {
            Id = _funko.Id,
            Name = request.Name,
            Franchise = request.Franchise,
            Number = request.Number,
            SupplierId = _supplier.Id,
            Category = _category.ToEntity(),
            CategoryId = _category.Id,
        };

        _repository
            .Setup(x => x.GetFunkoByIdAsync(_funko.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_funko);

        _repository
            .Setup(x => x.UpdateFunkoAsync(_funko.Id, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedFunko);

        _supplierService
            .Setup(x => x.GetSupplierByIdAsync(_supplier.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<SupplierDto, DomainError>(_supplier));

        // Act
        var result = await _service.UpdateFunkoAsync(_funko.Id, request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Name, Is.EqualTo(request.Name));
    }

    [Test]
    public async Task UpdatePriceAsync_WhenPriceDoesNotChange_ReturnsSuccess()
    {
        // Arrange
        var request = new FunkoUpdatePriceRequestDto(_funko.Price, 1);

        _repository
            .Setup(x => x.GetFunkoByIdAsync(_funko.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_funko);

        _repository
            .Setup(x => x.UpdatePriceFromFunkoAsync(
                _funko.Id,
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.UpdatePriceAsync(_funko.Id, request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.currentPrice, Is.EqualTo(_funko.Price));
    }

    [Test]
    public async Task UpdatePriceAsync_WhenPriceChanges_ReturnsSuccess()
    {
        // Arrange
        var newPrice = 25;
        var request = new FunkoUpdatePriceRequestDto(newPrice, 1);

        _repository
            .Setup(x => x.GetFunkoByIdAsync(_funko.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_funko);

        _repository
            .Setup(x => x.UpdatePriceFromFunkoAsync(
                _funko.Id,
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _historyService
            .Setup(x => x.AddHistoryAsync(
                _funko.Id,
                (decimal)newPrice,
                (decimal)_funko.Price,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<PriceHistoryModifiedResponse, DomainError>(
                new PriceHistoryModifiedResponse("Historial añadido")));

        // Act
        var result = await _service.UpdatePriceAsync(_funko.Id, request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.historyResponse.message, Is.EqualTo("Historial añadido"));
    }

    [Test]
    public async Task UpdateCategoryFromAsync_ReturnsUpdatedFunko()
    {
        // Arrange
        var request = new FunkoUpdateCategoryRequestDto(_category.Id, 1);

        var updatedFunko = new Funko
        {
            Id = _funko.Id,
            Name = _funko.Name,
            SupplierId = _supplier.Id,
            Category = _category.ToEntity(),
            CategoryId = _category.Id
        };

        _repository
            .Setup(x => x.UpdateCategoryFromFunkoAsync(
                _funko.Id,
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedFunko);

        _supplierService
            .Setup(x => x.GetSupplierByIdAsync(_supplier.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<SupplierDto, DomainError>(_supplier));

        // Act
        var result = await _service.UpdateCategoryFromAsync(_funko.Id, request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(_funko.Id));
    }

    [Test]
    public async Task UpdateSupplierFromFunko_ReturnsUpdatedFunko()
    {
        // Arrange
        var request = new FunkoUpdateSupplierDto(_supplier.Id, 1);

        var updatedFunko = new Funko
        {
            Id = _funko.Id,
            Name = _funko.Name,
            SupplierId = _supplier.Id,
            Category = _category.ToEntity(),
            CategoryId = _category.Id
        };

        _supplierService
            .Setup(x => x.GetSupplierByIdAsync(_supplier.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<SupplierDto, DomainError>(_supplier));

        _repository
            .Setup(x => x.UpdateSuplierFromFunkoAsync(
                _funko.Id,
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedFunko);

        // Act
        var result = await _service.UpdateSupplierFromFunko(_funko.Id, request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(_funko.Id));
    }

    // =========================================================
    // DELETE / RESTORE
    // =========================================================

    [Test]
    public async Task DeleteFunkoAsync_ReturnsSuccess()
    {
        // Arrange
        _repository
            .Setup(x => x.GetFunkoByIdAsync(_funko.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_funko);

        _repository
            .Setup(x => x.DeleteFunkoAsync(_funko.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _historyService
            .Setup(x => x.DeleteHistoryAsync(_funko.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(
                new PriceHistoryModifiedResponse("Historial eliminado")));

        // Act
        var result = await _service.DeleteFunkoAsync(_funko.Id);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.message, Is.EqualTo("Funko eliminado correctamente"));
    }

    [Test]
    public async Task RestoreDeletedFunkoAsync_ReturnsRestoredFunko()
    {
        // Arrange
        _repository
            .Setup(x => x.RestoreFunkoAsync(_funko.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_funko);

        _supplierService
            .Setup(x => x.GetSupplierByIdAsync(_supplier.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<SupplierDto, DomainError>(_supplier));

        // Act
        var result = await _service.RestoreDeletedFunkoAsync(_funko.Id);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo(_funko.Id));
    }

    // =========================================================
    // UPDATE IMAGE
    // =========================================================

    [Test]
    public async Task UpdateImageFromFunkoAsync_ReturnsUpdatedFunko()
    {
        // Arrange
        var file = new Mock<IFormFile>().Object;
        var newImage = "/uploads/funkos/nueva-imagen.png";

        _funko.Imagen = "/uploads/funkos/imagen-anterior.png";

        _repository
            .Setup(x => x.GetFunkoByIdAsync(_funko.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_funko);

        _storageService
            .Setup(x => x.DeleteFileAsync(_funko.Imagen))
            .ReturnsAsync(Result.Success<bool, DomainError>(true));

        _storageService
            .Setup(x => x.SaveFileAsync(file, "funkos"))
            .ReturnsAsync(Result.Success<string, DomainError>(newImage));

        _repository
            .Setup(x => x.UpdateImageFromFunkoAsync(
                _funko.Id,
                newImage,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _supplierService
            .Setup(x => x.GetSupplierByIdAsync(_supplier.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success<SupplierDto, DomainError>(_supplier));

        // Act
        var result = await _service.UpdateImageFromFunkoAsync(_funko.Id, file);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Image, Is.EqualTo(newImage));
    }
}