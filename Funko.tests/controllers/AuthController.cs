using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using ProductosApi.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private Mock<IAuthService> _authService = null!;
    private AuthController _controller = null!;

    private AuthResponse _authResponse = null!;

    [SetUp]
    public void SetUp()
    {
        // Arrange
        _authService = new Mock<IAuthService>();

        _controller = new AuthController(_authService.Object);

        _authResponse = new AuthResponse(
            "token-prueba",
            "refresh-token-prueba",
            "Bearer",
            900,
            1,
            "test",
            "USER"
        );
    }

    // =========================================================
    // SIGNUP
    // =========================================================

    [Test]
    public async Task Signup_ReturnsCreated()
    {
        // Arrange
        var request = new SingupRequest(
            "test",
            "test@test.com",
            "Password123"
        );

        _authService
            .Setup(s => s.SignupAsync(request))
            .ReturnsAsync(
                Result.Success<AuthResponse, DomainError>(_authResponse)
            );

        // Act
        var result = await _controller.Signup(request);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();

        var response = result.Result.Should()
            .BeOfType<ObjectResult>().Subject;

        response.StatusCode.Should().Be(StatusCodes.Status201Created);
        response.Value.Should().BeEquivalentTo(_authResponse);
    }

    [Test]
    public async Task Signup_WithExistingUser_ReturnsConflict()
    {
        // Arrange
        var request = new SingupRequest(
            "test",
            "test@test.com",
            "Password123"
        );

        _authService
            .Setup(s => s.SignupAsync(request))
            .ReturnsAsync(Result.Failure<AuthResponse, DomainError>(
                AuthError.DuplicateUser(request.Email)));

        // Act
        var result = await _controller.Signup(request);

        // Assert
        result.Result.Should().BeOfType<ConflictObjectResult>();
    }

    // =========================================================
    // LOGIN
    // =========================================================

    [Test]
    public async Task Login_ReturnsOk()
    {
        // Arrange
        var request = new LoginRequest(
            "test@test.com",
            "Password123"
        );

        _authService
            .Setup(s => s.LoginAsync(request))
            .ReturnsAsync(Result.Success<AuthResponse, DomainError>(_authResponse));

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();

        var response = result.Result.Should()
            .BeOfType<OkObjectResult>().Subject;

        response.Value.Should().BeEquivalentTo(_authResponse);
    }

    [Test]
    public async Task Login_WithIncorrectCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginRequest(
            "test@test.com",
            "PasswordIncorrecta"
        );

        _authService
            .Setup(s => s.LoginAsync(request))
            .ReturnsAsync(Result.Failure<AuthResponse, DomainError>(
                AuthError.InvalidCredentials()));

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }


    // =========================================================
    // SIGNUP ADMIN
    // =========================================================

    [Test]
    public async Task SignupAdmin_ReturnsCreated()
    {
        // Arrange
        var request = new SingupRequest(
            "admin",
            "admin@test.com",
            "Password123"
        );

        _authService
            .Setup(s => s.SignupAdminAsync(request))
            .ReturnsAsync(
                Result.Success<AuthResponse, DomainError>(_authResponse)
            );

        // Act
        var result = await _controller.SignupAdmin(request);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();

        var response = result.Result.Should()
            .BeOfType<ObjectResult>().Subject;

        response.StatusCode.Should().Be(StatusCodes.Status201Created);
        response.Value.Should().BeEquivalentTo(_authResponse);
    }

    [Test]
    public async Task SignupAdmin_WithExistingUser_ReturnsConflict()
    {
        // Arrange
        var request = new SingupRequest(
            "admin",
            "admin@test.com",
            "Password123"
        );

        _authService
            .Setup(s => s.SignupAdminAsync(request))
            .ReturnsAsync(
                Result.Failure<AuthResponse, DomainError>(
                    AuthError.DuplicateUser(request.Email))
            );

        // Act
        var result = await _controller.SignupAdmin(request);

        // Assert
        result.Result.Should().BeOfType<ConflictObjectResult>();
    }

}