
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MinimalApi.repositories;
using MinimalApi.Services.Email;
using Moq;
using NUnit.Framework;

[TestFixture]
public class InfrastructureConfigTests
{
    private ServiceCollection _services = null!;

    [SetUp]
    public void Setup()
    {
        _services = new ServiceCollection();
    }

    // =========================================================
    // REPOSITORIES CONFIG
    // =========================================================

    [Test]
    public void AddRepositories_ValidServices_RepositoriesRegistered()
    {
        // Act
        _services.AddRepositories();

        // Assert
        Assert.That(
            _services.Any(s => s.ServiceType == typeof(IFunkoRepository)),
            Is.True
        );

        Assert.That(
            _services.Any(s => s.ServiceType == typeof(ICategoryRepository)),
            Is.True
        );

        Assert.That(
            _services.Any(s => s.ServiceType == typeof(ISupplierRepository)),
            Is.True
        );

        Assert.That(
            _services.Any(s => s.ServiceType == typeof(IUserRepository)),
            Is.True
        );

        Assert.That(
            _services.Any(s => s.ServiceType == typeof(IFunkoPriceHistoryRepository)),
            Is.True
        );
    }

    // =========================================================
    // SERVICES CONFIG
    // =========================================================

    [Test]
    public void AddServices_ValidServices_ServicesRegistered()
    {
        // Act
        _services.AddServices();

        // Assert
        Assert.That(
            _services.Any(s => s.ServiceType == typeof(IFunkoService)),
            Is.True
        );

        Assert.That(
            _services.Any(s => s.ServiceType == typeof(ISupplierService)),
            Is.True
        );

        Assert.That(
            _services.Any(s => s.ServiceType == typeof(ICategoryService)),
            Is.True
        );

        Assert.That(
            _services.Any(s => s.ServiceType == typeof(ICache)),
            Is.True
        );

        Assert.That(
            _services.Any(s => s.ServiceType == typeof(IAuthService)),
            Is.True
        );

        Assert.That(
            _services.Any(s => s.ServiceType == typeof(IJwtService)),
            Is.True
        );

        Assert.That(
            _services.Any(s => s.ServiceType == typeof(IFunkoPriceHistoryService)),
            Is.True
        );

        Assert.That(
            _services.Any(s => s.ServiceType == typeof(INotificationService)),
            Is.True
        );

        Assert.That(
            _services.Any(s => s.ServiceType == typeof(IEmailService)),
            Is.True
        );

        Assert.That(
            _services.Any(s => s.ServiceType == typeof(IEmailQueueService)),
            Is.True
        );

        Assert.That(
            _services.Any(s => s.ServiceType == typeof(IUserService)),
            Is.True
        );
    }

    // =========================================================
    // SIGNALR CONFIG
    // =========================================================

    [Test]
    public void AddSignalRconfiguration_ValidServices_SignalRRegistered()
    {
        // Act
        _services.AddSignalRconfiguration();

        // Assert
        Assert.That(
            _services.Any(s => s.ServiceType == typeof(IHubContext<>)),
            Is.True
        );
    }

    // =========================================================
    // STORAGE CONFIG
    // =========================================================

    [Test]
    public void AddStorage_ValidServices_StorageServiceRegistered()
    {
        // Act
        _services.AddStorage();

        // Assert
        var descriptor = _services.FirstOrDefault(
            s => s.ServiceType == typeof(IStorageService)
        );

        Assert.That(descriptor, Is.Not.Null);
        Assert.That(
            descriptor!.ImplementationType,
            Is.EqualTo(typeof(FileSystemStorageService))
        );
        Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
    }

    // =========================================================
    // AUTHENTICATION CONFIG
    // =========================================================

    [Test]
    public void AddAuthenticationJwt_ValidConfiguration_AuthenticationRegistered()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "EstaEsUnaClaveDePruebaDeAlMenos32Caracteres!",
            ["Jwt:Issuer"] = "FunkoApi",
            ["Jwt:Audience"] = "FunkoUsers"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        _services.AddLogging();

        // Act
        _services.AddAuthenticationJwt(configuration);

        // Assert
        Assert.That(
            _services.Any(s => s.ServiceType == typeof(IAuthenticationService)),
            Is.True
        );

        Assert.That(
            _services.Any(s => s.ServiceType == typeof(IAuthorizationService)),
            Is.True
        );

        Assert.That(
            _services.Any(s => s.ServiceType == typeof(JwtService)),
            Is.True
        );

        Assert.That(
            _services.Any(s => s.ServiceType == typeof(AuthService)),
            Is.True
        );
    }

    [Test]
    public void AddAuthenticationJwt_MissingSecret_InvalidOperationException()
    {
        // Arrange
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => _services.AddAuthenticationJwt(configuration)
        );
    }

    // =========================================================
    // GRAPHQL CONFIG
    // =========================================================

    [Test]
    public void AddGraphQL_DevelopmentEnvironment_GraphQLRegistered()
    {
        // Arrange
        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(e => e.EnvironmentName)
            .Returns("Development");

        // Act
        var result = _services.AddGraphQL(environment.Object);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    // =========================================================
    // DATABASE CONFIG
    // =========================================================

    [Test]
    public void AddDatabases_ValidConfiguration_DbContextsRegistered()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["DbConnections:Postgre:ConnectionStrings:DefaultConnection"] =
                "Host=localhost;Database=FunkoTest;Username=test;Password=test",

            ["DbConnections:Mongo:ConnectionStrings:DefaultConnection"] =
                "mongodb://localhost:27017/FunkoTest"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        // Act
        _services.AddDatabases(configuration);

        // Assert
        Assert.That(
            _services.Any(s => s.ServiceType == typeof(AppDbContext)),
            Is.True
        );

        Assert.That(
            _services.Any(s => s.ServiceType == typeof(AppDbMongoContext)),
            Is.True
        );
    }
}
