
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Moq;

[TestFixture]
public class SupplierServiceTests
{
    private Mock<ISupplierRepository> _repository = null!;
    private Mock<ILogger<SupplierService>> _logger = null!;
    private Mock<INotificationService> _notificationService = null!;
    private Mock<ICache> _cache = null!;
    private SupplierService _service = null!;

    [SetUp]
    public void Setup()
    {
        _repository = new Mock<ISupplierRepository>();
        _logger = new Mock<ILogger<SupplierService>>();
        _notificationService = new Mock<INotificationService>();
        _cache = new Mock<ICache>();

        _service = new SupplierService(
            _repository.Object,
            _logger.Object,
            _cache.Object,
            _notificationService.Object
        );
    }

    public Supplier CreateSupplierValid() => new Supplier
    {
        Id = YoutubeIdValueGenerator.GenerateId(),
        Name = "Proveedor de prueba"
    };

    // =========================================================
    // CREATE
    // =========================================================

    [Test]
    public async Task CreateSupplier_ValidSupplier_Success()
    {
        // Arrange
        var supplier = CreateSupplierValid();

        var request = new SupplierCreateRequestDto(
            supplier.Name
        );

        _repository
            .Setup(r => r.ExistByNameAsync(
                request.Name,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(false);

        _repository
            .Setup(r => r.CreateSupplierAsync(
                request,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(supplier);

        // Act
        var response = await _service.CreateSupplierAsync(request);

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value.Name, Is.EqualTo(supplier.Name));
        Assert.That(response.Value.Id, Is.EqualTo(supplier.Id));
    }

    [Test]
    public async Task CreateSupplier_ExistingName_Failure()
    {
        // Arrange
        var request = new SupplierCreateRequestDto(
            "Proveedor existente"
        );

        _repository
            .Setup(r => r.ExistByNameAsync(
                request.Name,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(true);

        // Act
        var response = await _service.CreateSupplierAsync(request);

        // Assert
        Assert.That(response.IsFailure, Is.True);

        _repository.Verify(
            r => r.CreateSupplierAsync(
                It.IsAny<SupplierCreateRequestDto>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
    }

    // =========================================================
    // GET ALL
    // =========================================================

    [Test]
    public async Task GetAllSuppliers_ExistingSuppliers_Success()
    {
        // Arrange
        var suppliers = new List<Supplier>
        {
            CreateSupplierValid(),
            CreateSupplierValid()
        };

        _repository
            .Setup(r => r.GetAllSuppliersAsync(
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(suppliers);

        // Act
        var response = await _service.GetAllSuppliersAsync();

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetAllSuppliers_NoSuppliers_EmptyList()
    {
        // Arrange
        _repository
            .Setup(r => r.GetAllSuppliersAsync(
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(new List<Supplier>());

        // Act
        var response = await _service.GetAllSuppliersAsync();

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value, Is.Empty);
    }

    // =========================================================
    // PAGINATION
    // =========================================================

    [Test]
    public async Task GetPaginatedSuppliers_FirstPage_Success()
    {
        // Arrange
        var suppliers = new List<Supplier>
        {
            CreateSupplierValid(),
            CreateSupplierValid()
        };

        _repository
            .Setup(r => r.GetPaginatedSuppliersAsync(
                1,
                2,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((suppliers, 2, 1, false, false));

        // Act
        var response = await _service.GetPaginatedSuppliersAsync(1, 2);

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value.Items, Has.Count.EqualTo(2));
        Assert.That(response.Value.TotalPages, Is.EqualTo(1));
    }

   
    [Test]
    public async Task GetPaginatedSuppliers_SecondPage_Success()
    {
        // Arrange
        var suppliers = new List<Supplier>
        {
            CreateSupplierValid()
        };

        _repository
            .Setup(r => r.GetPaginatedSuppliersAsync(
                2,
                2,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((suppliers, 3, 2, true, false));

        // Act
        var response = await _service.GetPaginatedSuppliersAsync(2, 2);

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value.Items, Has.Count.EqualTo(1));
        Assert.That(response.Value.HasPreviusPage, Is.True);

        _cache.Verify(
            c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<PaginatedResouce<SupplierDto>>()
            ),
            Times.Never
        );
    }

    [Test]
    public async Task GetPaginatedSuppliers_EmptyPage_DoesNotCache()
    {
        // Arrange
        _repository
            .Setup(r => r.GetPaginatedSuppliersAsync(
                1,
                10,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((
                new List<Supplier>(),
                0,
                0,
                false,
                false
            ));

        // Act
        var response = await _service.GetPaginatedSuppliersAsync(1, 10);

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value.Items, Is.Empty);

        _cache.Verify(
            c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<PaginatedResouce<SupplierDto>>()
            ),
            Times.Never
        );
    }

    // =========================================================
    // GET BY ID
    // =========================================================

    [Test]
    public async Task GetSupplierById_ExistingSupplier_Success()
    {
        // Arrange
        var supplier = CreateSupplierValid();

        _repository
            .Setup(r => r.GetSupplierByIdAsync(
                supplier.Id,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(supplier);

        // Act
        var response = await _service.GetSupplierByIdAsync(supplier.Id);

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value.Id, Is.EqualTo(supplier.Id));

        _cache.Verify(
            c => c.SetAsync(
                $"SUPPLIER:{supplier.Id}",
                supplier
            ),
            Times.Once
        );
    }

    [Test]
    public async Task GetSupplierById_CachedSupplier_Success()
    {
        // Arrange
        var supplier = CreateSupplierValid();

        _cache
            .Setup(c => c.GetTAsync<Supplier>(
                $"SUPPLIER:{supplier.Id}"
            ))
            .ReturnsAsync(supplier);

        // Act
        var response = await _service.GetSupplierByIdAsync(supplier.Id);

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value.Id, Is.EqualTo(supplier.Id));

        _repository.Verify(
            r => r.GetSupplierByIdAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Never
        );
    }

    [Test]
    public async Task GetSupplierById_NonExistingSupplier_Failure()
    {
        // Arrange
        var id = "proveedor_inexistente";

        _cache
            .Setup(c => c.GetTAsync<Supplier>($"SUPPLIER:{id}"))
            .ReturnsAsync((Supplier?)null);

        _repository
            .Setup(r => r.GetSupplierByIdAsync(
                id,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((Supplier?)null);

        // Act
        var response = await _service.GetSupplierByIdAsync(id);

        // Assert
        Assert.That(response.IsFailure, Is.True);

        _cache.Verify(
            c => c.SetAsync(
                It.IsAny<string>(),
                It.IsAny<Supplier>()
            ),
            Times.Never
        );
    }

    // =========================================================
    // UPDATE
    // =========================================================

    [Test]
    public async Task UpdateSupplier_ExistingSupplier_Success()
    {
        // Arrange
        var supplier = CreateSupplierValid();

        var request = new SupplierUpdateRequestDto(
            "Proveedor actualizado",
            supplier.RowVersion
        );

        supplier.Name = request.Name;

        _repository
            .Setup(r => r.UpdateSupplierAsync(
                supplier.Id,
                request,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(supplier);

        // Act
        var response = await _service.UpdateSupplierAsync(
            supplier.Id,
            request
        );

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value.Name, Is.EqualTo(request.Name));
    }

    [Test]
    public async Task UpdateSupplier_NonExistingSupplier_Failure()
    {
        // Arrange
        var id = "proveedor_inexistente";

        var request = new SupplierUpdateRequestDto(
            "Proveedor actualizado",
            1
        );

        _repository
            .Setup(r => r.UpdateSupplierAsync(
                id,
                request,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync((Supplier?)null);

        // Act
        var response = await _service.UpdateSupplierAsync(id, request);

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
    public async Task DeleteSupplier_ExistingSupplier_Success()
    {
        // Arrange
        var supplier = CreateSupplierValid();

        _repository
            .Setup(r => r.DeleteSupplierAsync(
                supplier.Id,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(true);

        // Act
        var response = await _service.DeleteSupplierAsync(supplier.Id);

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value, Is.True);
    }

    [Test]
    public async Task DeleteSupplier_NonExistingSupplier_Failure()
    {
        // Arrange
        var id = "proveedor_inexistente";

        _repository
            .Setup(r => r.DeleteSupplierAsync(
                id,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(false);

        // Act
        var response = await _service.DeleteSupplierAsync(id);

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
    public async Task RestoreSupplier_DeletedSupplier_Success()
    {
        // Arrange
        var supplier = CreateSupplierValid();

        _repository
            .Setup(r => r.RestoreSupplierAsync(
                supplier.Id,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(true);

        // Act
        var response = await _service.RestoreSupplierAsync(supplier.Id);

        // Assert
        Assert.That(response.IsSuccess, Is.True);
        Assert.That(response.Value, Is.True);
    }

    [Test]
    public async Task RestoreSupplier_NonExistingSupplier_Failure()
    {
        // Arrange
        var id = "proveedor_inexistente";

        _repository
            .Setup(r => r.RestoreSupplierAsync(
                id,
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(false);

        // Act
        var response = await _service.RestoreSupplierAsync(id);

        // Assert
        Assert.That(response.IsFailure, Is.True);
    }
}
