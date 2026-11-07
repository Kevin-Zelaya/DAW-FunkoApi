
using Cassandra;
using MinimalApi.repositories;
using NUnit.Framework;
using Testcontainers.Cassandra;
using ISession = Cassandra.ISession;

[TestFixture]
public class FunkoPriceHistoryRepositoryTests
{
    private CassandraContainer _container = null!;
    private ICluster _cluster = null!;
    private ISession _session = null!;
    private FunkoPriceHistoryRepository _repository = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _container = new CassandraBuilder()
            .WithImage("cassandra:4.1")
            .Build();

        await _container.StartAsync();

        _cluster = Cluster.Builder()
            .AddContactPoint(_container.Hostname)
            .WithPort(_container.GetMappedPublicPort(9042))
            .Build();

        _session = await _cluster.ConnectAsync();

        await _session.ExecuteAsync(new SimpleStatement(@"
            CREATE KEYSPACE IF NOT EXISTS funko_test
            WITH replication = {
                'class': 'SimpleStrategy',
                'replication_factor': 1
            };
        "));

        await _session.ExecuteAsync(new SimpleStatement(@"
            CREATE TABLE IF NOT EXISTS funko_test.funko_price_history (
                funko_id bigint,
                date timestamp,
                price decimal,
                old_price decimal,
                deleted boolean,
                PRIMARY KEY (funko_id, date)
            ) WITH CLUSTERING ORDER BY (date DESC);
        "));

        _session.ChangeKeyspace("funko_test");
    }

    [SetUp]
    public async Task Setup()
    {
        await _session.ExecuteAsync(
            new SimpleStatement("TRUNCATE funko_price_history;")
        );

        _repository = new FunkoPriceHistoryRepository(_session);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        _session.Dispose();
        _cluster.Dispose();
        await _container.DisposeAsync();
    }

    // =========================================================
    // ADD
    // =========================================================

    [Test]
    public async Task AddPrice_ValidPrice_PriceStored()
    {
        // Arrange
        long funkoId = 1;
        decimal newPrice = 25.99m;
        decimal oldPrice = 19.99m;

        // Act
        await _repository.AddPriceAsync(
            funkoId,
            newPrice,
            oldPrice,
            CancellationToken.None
        );

        var response = await _repository.GetHistoryByFunkoIdAsync(
            funkoId,
            CancellationToken.None
        );

        // Assert
        Assert.That(response, Has.Count.EqualTo(1));
        Assert.That(response[0].FunkoId, Is.EqualTo(funkoId));
        Assert.That(response[0].Price, Is.EqualTo(newPrice));
        Assert.That(response[0].OldPrice, Is.EqualTo(oldPrice));
    }

    [Test]
    public async Task AddPrice_MultiplePrices_AllPricesStored()
    {
        // Arrange
        long funkoId = 1;

        // Act
        await _repository.AddPriceAsync(
            funkoId, 20m, 15m, CancellationToken.None
        );

        await Task.Delay(10);

        await _repository.AddPriceAsync(
            funkoId, 25m, 20m, CancellationToken.None
        );

        var response = await _repository.GetHistoryByFunkoIdAsync(
            funkoId,
            CancellationToken.None
        );

        // Assert
        Assert.That(response, Has.Count.EqualTo(2));
        Assert.That(response.Any(p => p.Price == 20m), Is.True);
        Assert.That(response.Any(p => p.Price == 25m), Is.True);
    }

    // =========================================================
    // GET
    // =========================================================

    [Test]
    public async Task GetHistoryByFunkoId_ExistingHistory_HistoryReturned()
    {
        // Arrange
        long funkoId = 1;

        await _repository.AddPriceAsync(
            funkoId, 30m, 20m, CancellationToken.None
        );

        // Act
        var response = await _repository.GetHistoryByFunkoIdAsync(
            funkoId,
            CancellationToken.None
        );

        // Assert
        Assert.That(response, Has.Count.EqualTo(1));
        Assert.That(response[0].Price, Is.EqualTo(30m));
    }

    [Test]
    public async Task GetHistoryByFunkoId_NonExistingFunko_EmptyList()
    {
        // Arrange
        long funkoId = 999;

        // Act
        var response = await _repository.GetHistoryByFunkoIdAsync(
            funkoId,
            CancellationToken.None
        );

        // Assert
        Assert.That(response, Is.Empty);
    }

    [Test]
    public async Task GetHistoryByFunkoId_DifferentFunkos_OnlyRequestedHistory()
    {
        // Arrange
        await _repository.AddPriceAsync(
            1, 20m, 15m, CancellationToken.None
        );

        await _repository.AddPriceAsync(
            2, 40m, 30m, CancellationToken.None
        );

        // Act
        var response = await _repository.GetHistoryByFunkoIdAsync(
            1,
            CancellationToken.None
        );

        // Assert
        Assert.That(response, Has.Count.EqualTo(1));
        Assert.That(response[0].FunkoId, Is.EqualTo(1));
        Assert.That(response[0].Price, Is.EqualTo(20m));
    }

    // =========================================================
    // DELETE
    // =========================================================

    [Test]
    public async Task DeleteHistory_ExistingHistory_HistoryHidden()
    {
        // Arrange
        long funkoId = 1;

        await _repository.AddPriceAsync(
            funkoId, 25m, 20m, CancellationToken.None
        );

        // Act
        await _repository.DeleteHistoryByFunkoId(
            funkoId,
            CancellationToken.None
        );

        var response = await _repository.GetHistoryByFunkoIdAsync(
            funkoId,
            CancellationToken.None
        );

        // Assert
        Assert.That(response, Is.Empty);
    }

    // =========================================================
    // RESTORE
    // =========================================================

    [Test]
    public async Task RestoreHistory_DeletedHistory_HistoryRestored()
    {
        // Arrange
        long funkoId = 1;

        await _repository.AddPriceAsync(
            funkoId, 25m, 20m, CancellationToken.None
        );

        await _repository.DeleteHistoryByFunkoId(
            funkoId,
            CancellationToken.None
        );

        // Act
        await _repository.RestoreHistoryByFunkoId(
            funkoId,
            CancellationToken.None
        );

        var response = await _repository.GetHistoryByFunkoIdAsync(
            funkoId,
            CancellationToken.None
        );

        // Assert
        Assert.That(response, Has.Count.EqualTo(1));
        Assert.That(response[0].Price, Is.EqualTo(25m));
    }
}
