
using MinimalApi.models;
using MinimalApi.repositories;
using Moq;
using NUnit.Framework;
using System.Net;

[TestFixture]
public class FunkoPriceHistoryServiceTests
{
    private Mock<IFunkoPriceHistoryRepository> _repositoryMock = null!;
    private FunkoPriceHistoryService _service = null!;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IFunkoPriceHistoryRepository>();
        _service = new FunkoPriceHistoryService(_repositoryMock.Object);
    }

    // =========================================================
    // HELPERS
    // =========================================================

    public List<PriceHistoryDto> CreateHistoryValid()
    {
        return new List<PriceHistoryDto>
        {
            new PriceHistoryDto
            {
                FunkoId = 1,
                Date = DateTime.UtcNow.AddDays(-2),
                Price = 20,
                OldPrice = 15
            },
            new PriceHistoryDto
            {
                FunkoId = 1,
                Date = DateTime.UtcNow.AddDays(-1),
                Price = 25,
                OldPrice = 20
            },
            new PriceHistoryDto
            {
                FunkoId = 1,
                Date = DateTime.UtcNow,
                Price = 18,
                OldPrice = 25
            }
        };
    }

    // =========================================================
    // ADD HISTORY
    // =========================================================

    [Test]
    public async Task AddHistoryAsync_ValidData_ReturnsSuccess()
    {
        // Arrange
        long funkoId = 1;
        decimal price = 20;
        decimal oldPrice = 15;

        _repositoryMock
            .Setup(r => r.AddPriceAsync(
                funkoId, price, oldPrice, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddHistoryAsync(
            funkoId, price, oldPrice, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.message,
            Is.EqualTo($"Precio agregado correctamente para el funko con id {funkoId}"));

        _repositoryMock.Verify(r => r.AddPriceAsync(
            funkoId, price, oldPrice, It.IsAny<CancellationToken>()), Times.Once);
    }

    // =========================================================
    // GET HISTORY
    // =========================================================

    [Test]
    public async Task GetHistoryAsync_ExistingHistory_ReturnsHistory()
    {
        // Arrange
        long funkoId = 1;
        var history = CreateHistoryValid();

        _repositoryMock
            .Setup(r => r.GetHistoryByFunkoIdAsync(
                funkoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        // Act
        var result = await _service.GetHistoryAsync(
            funkoId, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Prices, Has.Count.EqualTo(3));
        Assert.That(result.Value.MaxPrice, Is.EqualTo(25));
        Assert.That(result.Value.MinPrice, Is.EqualTo(18));

        _repositoryMock.Verify(r => r.GetHistoryByFunkoIdAsync(
            funkoId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task GetHistoryAsync_EmptyHistory_ReturnsNotFound()
    {
        // Arrange
        long funkoId = 1;

        _repositoryMock
            .Setup(r => r.GetHistoryByFunkoIdAsync(
                funkoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PriceHistoryDto>());

        // Act
        var result = await _service.GetHistoryAsync(
            funkoId, CancellationToken.None);

        // Assert
        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.TypeOf<NotFoundError>());
        Assert.That(result.Error.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    // =========================================================
    // DELETE HISTORY
    // =========================================================

    [Test]
    public async Task DeleteHistoryAsync_ValidFunko_ReturnsSuccess()
    {
        // Arrange
        long funkoId = 1;

        _repositoryMock
            .Setup(r => r.DeleteHistoryByFunkoId(
                funkoId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteHistoryAsync(
            funkoId, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.message,
            Is.EqualTo("Eliminado el historial de precios vinculado al funko."));

        _repositoryMock.Verify(r => r.DeleteHistoryByFunkoId(
            funkoId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteHistoryAsync_RepositoryThrows_ReturnsErrorMessage()
    {
        // Arrange
        long funkoId = 1;

        _repositoryMock
            .Setup(r => r.DeleteHistoryByFunkoId(
                funkoId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error Cassandra"));

        // Act
        var result = await _service.DeleteHistoryAsync(
            funkoId, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.message,
            Is.EqualTo("Error al eliminar el historial de precio vinculado al usuario"));
    }

    // =========================================================
    // RESTORE HISTORY
    // =========================================================

    [Test]
    public async Task RestoreHistoryAsync_ValidFunko_ReturnsSuccess()
    {
        // Arrange
        long funkoId = 1;

        _repositoryMock
            .Setup(r => r.RestoreHistoryByFunkoId(
                funkoId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RestoreHistoryAsync(
            funkoId, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.message,
            Is.EqualTo("Restaurado el historial de precios vinculado al funko."));

        _repositoryMock.Verify(r => r.RestoreHistoryByFunkoId(
            funkoId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RestoreHistoryAsync_RepositoryThrows_ReturnsErrorMessage()
    {
        // Arrange
        long funkoId = 1;

        _repositoryMock
            .Setup(r => r.RestoreHistoryByFunkoId(
                funkoId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Error Cassandra"));

        // Act
        var result = await _service.RestoreHistoryAsync(
            funkoId, CancellationToken.None);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.message,
            Is.EqualTo("Error al restaurar el historial de precio vinculado al usuario"));
    }
}
