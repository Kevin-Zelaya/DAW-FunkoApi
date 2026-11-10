using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

[TestFixture]
public class JwtServiceTests
{
    private JwtService _service = null!;

    private const string Secret =
        "EstaEsUnaClaveSecretaDePruebaDeAlMenos32Caracteres";

    [SetUp]
    public void SetUp()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = Secret,
            ["Jwt:Issuer"] = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience",
            ["Jwt:ExpirationMinutes"] = "15",
            ["Jwt:RefreshTokenExpiryDays"] = "7"
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        _service = new JwtService(configuration);
    }

    // =========================================================
    // GENERATE TOKEN
    // =========================================================

    [Test]
    public void GenerateToken_ReturnsValidToken()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            Username = "test",
            Role = Role.USER
        };

        // Act
        var token = _service.GenerateToken(user);

        // Assert
        Assert.That(token, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void GenerateToken_ContainsUserClaims()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            Username = "test",
            Role = Role.USER
        };

        // Act
        var token = _service.GenerateToken(user);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // Assert
        Assert.That(jwt.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value,
            Is.EqualTo(user.Id.ToString()));

        Assert.That(jwt.Claims.First(x => x.Type == JwtRegisteredClaimNames.Email).Value,
            Is.EqualTo(user.Email));

        Assert.That(jwt.Claims.First(x => x.Type == "username").Value,
            Is.EqualTo(user.Username));

        Assert.That(jwt.Claims.First(x => x.Type == "role").Value,
            Is.EqualTo(user.Role.ToString()));
    }

    [Test]
    public void AccessTokenExpirySeconds_ReturnsConfiguredExpiration()
    {
        // Arrange
        var expectedSeconds = 15 * 60;

        // Act
        var result = _service.AccessTokenExpirySeconds;

        // Assert
        Assert.That(result, Is.EqualTo(expectedSeconds));
    }

    // =========================================================
    // GENERATE REFRESH TOKEN
    // =========================================================

    [Test]
    public void GenerateRefreshToken_ReturnsToken()
    {
        // Arrange

        // Act
        var token = _service.GenerateRefreshToken();

        // Assert
        Assert.That(token, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void GenerateRefreshToken_GeneratesDifferentTokens()
    {
        // Arrange

        // Act
        var token1 = _service.GenerateRefreshToken();
        var token2 = _service.GenerateRefreshToken();

        // Assert
        Assert.That(token1, Is.Not.EqualTo(token2));
    }

    // =========================================================
    // VALIDATE TOKEN
    // =========================================================

    [Test]
    public void ValidateToken_WithValidToken_ReturnsClaimsPrincipal()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Email = "test@test.com",
            Username = "test",
            Role = Role.USER
        };

        var token = _service.GenerateToken(user);

        // Act
        var result = _service.validateToken(token);

        // Assert
        Assert.That(result, Is.Not.Null);
    }

    [Test]
    public void ValidateToken_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var token = "token-invalido";

        // Act
        var result = _service.validateToken(token);

        // Assert
        Assert.That(result, Is.Null);
    }

    // =========================================================
    // CONFIGURATION
    // =========================================================

    [Test]
    public void Constructor_WithoutSecret_ThrowsException()
    {
        // Arrange
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act y Assert
        Assert.That(
            () => new JwtService(configuration),
            Throws.InvalidOperationException
        );
    }
}