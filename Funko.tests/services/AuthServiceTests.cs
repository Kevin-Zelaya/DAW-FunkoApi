using Moq;
using NUnit.Framework;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IUserRepository> _userRepository = null!;
    private Mock<IJwtService> _jwtService = null!;

    private AuthService _service = null!;

    private User _user = null!;

    [SetUp]
    public void SetUp()
    {
        // Arrange
        _userRepository = new Mock<IUserRepository>();
        _jwtService = new Mock<IJwtService>();

        _service = new AuthService(
            _userRepository.Object,
            _jwtService.Object
        );

        _user = new User
        {
            Id = 1,
            Username = "test",
            Email = "test@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123", 11),
            Role = Role.USER
        };

        _jwtService
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("token-prueba");

        _jwtService
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token-prueba");

        _jwtService
            .SetupGet(x => x.AccessTokenExpirySeconds)
            .Returns(900);
    }

    // =========================================================
    // SIGNUP
    // =========================================================

    [Test]
    public async Task SignupAsync_WithNewEmail_ReturnsSuccess()
    {
        // Arrange
        var request = new SingupRequest(
            "test",
            "test@test.com",
            "Password123"
        );

        _userRepository
            .Setup(x => x.GetUserByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        _userRepository
            .Setup(x => x.CreateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(_user);

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Username, Is.EqualTo(_user.Username));
        Assert.That(result.Value.Role, Is.EqualTo(_user.Role.ToString()));
    }

    [Test]
    public async Task SignupAsync_WithExistingEmail_ReturnsFailure()
    {
        // Arrange
        var request = new SingupRequest(
            "test",
            "test@test.com",
            "Password123"
        );

        _userRepository
            .Setup(x => x.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(_user);

        // Act
        var result = await _service.SignupAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
    }

    // =========================================================
    // LOGIN
    // =========================================================

    [Test]
    public async Task LoginAsync_WithCorrectCredentials_ReturnsSuccess()
    {
        // Arrange
        var request = new LoginRequest(
            "test@test.com",
            "Password123"
        );

        _userRepository
            .Setup(x => x.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(_user);

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Username, Is.EqualTo(_user.Username));
        Assert.That(result.Value.Role, Is.EqualTo(_user.Role.ToString()));
    }

    [Test]
    public async Task LoginAsync_WithIncorrectPassword_ReturnsFailure()
    {
        // Arrange
        var request = new LoginRequest(
            "test@test.com",
            "PasswordIncorrecta"
        );

        _userRepository
            .Setup(x => x.GetUserByEmailAsync(request.Email))
            .ReturnsAsync(_user);

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
    }

    [Test]
    public async Task LoginAsync_WithNonExistingEmail_ReturnsFailure()
    {
        // Arrange
        var request = new LoginRequest(
            "desconocido@test.com",
            "Password123"
        );

        _userRepository
            .Setup(x => x.GetUserByEmailAsync(request.Email))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.LoginAsync(request);

        // Assert
        Assert.That(result.IsFailure, Is.True);
    }
}