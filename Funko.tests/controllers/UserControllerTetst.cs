
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.Http;
using CSharpFunctionalExtensions;

[TestFixture]
public class UserControllerTests
{
    private Mock<IUserService> _userService = null!;
    private UserController _controller = null!;

    [SetUp]
    public void SetUp()
    {
        _userService = new Mock<IUserService>();
        _controller = new UserController(_userService.Object);
    }

    private void SetUser(long? id = null, string? role = "USER")
    {
        var claims = new List<Claim>();

        if (id.HasValue)
            claims.Add(new Claim(
                JwtRegisteredClaimNames.Sub,
                id.Value.ToString()));

        if (role != null)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var identity = new ClaimsIdentity(
            claims,
            "TestAuthentication");

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };
    }

    private static UserDto CreateUser() =>
        new(
            1,
            "Kevin",
            "kevin@email.com",
            Role.USER.ToString(),
            false,
            DateTime.UtcNow,
            null);

    private static Result<T, DomainError> Success<T>(T value) =>
        Result.Success<T, DomainError>(value);

    private static Result<T, DomainError> Failure<T>() =>
        Result.Failure<T, DomainError>(
            new BusinessRuleError("Error de prueba"));

    #region GET

    [Test]
    public async Task GetUserById_Success_ReturnsOk()
    {
        var user = CreateUser();

        _userService
            .Setup(x => x.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success<UserDto>(user));

        var result = await _controller.GetUserById(
            1, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
        _userService.Verify(
            x => x.GetUserByIdAsync(1, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task GetUserById_Error_ReturnsErrorResult()
    {
        _userService
            .Setup(x => x.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Failure<UserDto>());

        var result = await _controller.GetUserById(
            1, CancellationToken.None);

        Assert.That(result.Result, Is.Not.Null);
        Assert.That(result.Result, Is.Not.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetMe_WithoutUserId_ReturnsUnauthorized()
    {
        SetUser();

        var result = await _controller.GetMe(CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<UnauthorizedResult>());
        _userService.Verify(
            x => x.GetUserByIdAsync(
                It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task GetMe_Success_ReturnsOk()
    {
        SetUser(1);

        _userService
            .Setup(x => x.GetUserByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success<UserDto>(CreateUser()));

        var result = await _controller.GetMe(CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetUserByEmail_Success_ReturnsOk()
    {
        _userService
            .Setup(x => x.GetUserByEmailAsync(
                "kevin@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success<UserDto>(CreateUser()));

        var result = await _controller.GetUserByEmail(
            "kevin@email.com", CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetAllUsers_Success_ReturnsOk()
    {
        var users = new List<UserDto> { CreateUser() };

        _userService
            .Setup(x => x.GetAllUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success<List<UserDto>>(users));

        var result = await _controller.GetAllUsers(
            CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task GetPaginatedUsers_Success_ReturnsOk()
    {
        _userService
            .Setup(x => x.GetPaginatedUsersAsync(
                1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success<PaginatedResouce<UserDto>>(default!));

        var result = await _controller.GetPaginatedUsers(
            1, 10, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    #endregion

    #region UPDATE

    [Test]
    public async Task UpdateUser_Success_ReturnsOk()
    {
        var request = default(UserUpdateRegularRequestDto)!;

        _userService
            .Setup(x => x.UpdateUserAsync(
                1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success<UserDto>(CreateUser()));

        var result = await _controller.UpdateUser(
            1, request, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdateMe_WithoutUserId_ReturnsUnauthorized()
    {
        SetUser();

        var result = await _controller.UpdateMe(
            default!, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<UnauthorizedResult>());

        _userService.Verify(
            x => x.UpdateUserAsync(
                It.IsAny<long>(),
                It.IsAny<UserUpdateRegularRequestDto>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task UpdateMe_Success_ReturnsOk()
    {
        SetUser(1);

        var request = default(UserUpdateRegularRequestDto)!;

        _userService
            .Setup(x => x.UpdateUserAsync(
                1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success<UserDto>(CreateUser()));

        var result = await _controller.UpdateMe(
            request, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdatePassword_WithoutUserId_ReturnsUnauthorized()
    {
        SetUser();

        var result = await _controller.UpdatePassword(
            1, "newPassword", CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<UnauthorizedResult>());
    }

    [Test]
    public async Task UpdatePassword_OwnAccount_ReturnsOk()
    {
        SetUser(1);

        _userService
            .Setup(x => x.UpdatePasswordAsync(
                1, "newPassword", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success<UserDto>(CreateUser()));

        var result = await _controller.UpdatePassword(
            1, "newPassword", CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdatePassword_OtherAccountWithoutAdminRole_ReturnsForbid()
    {
        SetUser(1, "USER");

        var result = await _controller.UpdatePassword(
            2, "newPassword", CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<ForbidResult>());

        _userService.Verify(
            x => x.UpdatePasswordAsync(
                It.IsAny<long>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task UpdatePassword_AdminCanUpdateOtherAccount()
    {
        SetUser(1, "ADMIN");

        _userService
            .Setup(x => x.UpdatePasswordAsync(
                2, "newPassword", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success<UserDto>(CreateUser()));

        var result = await _controller.UpdatePassword(
            2, "newPassword", CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    [Test]
    public async Task UpdateRole_Success_ReturnsOk()
    {
        _userService
            .Setup(x => x.UpdateRoleAsync(
                1, "ADMIN", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success<UserDto>(CreateUser()));

        var result = await _controller.UpdateRole(
            1, "ADMIN", CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    #endregion

    #region DELETE

    [Test]
    public async Task DeleteUser_Success_ReturnsNoContent()
    {
        _userService
            .Setup(x => x.DeleteUserAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success<bool>(true));

        var result = await _controller.DeleteUser(
            1, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<NoContentResult>());
    }

    [Test]
    public async Task DeleteMe_WithoutUserId_ReturnsUnauthorized()
    {
        SetUser();

        var result = await _controller.DeleteMe(CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<UnauthorizedResult>());

        _userService.Verify(
            x => x.DeleteUserAsync(
                It.IsAny<long>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task DeleteMe_Success_ReturnsNoContent()
    {
        SetUser(1);

        _userService
            .Setup(x => x.DeleteUserAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success<bool>(true));

        var result = await _controller.DeleteMe(CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<NoContentResult>());
    }

    #endregion

    #region RESTORE

    [Test]
    public async Task RestoreUser_Success_ReturnsOk()
    {
        _userService
            .Setup(x => x.RestoreUserAsync(
                1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Success<UserDto>(CreateUser()));

        var result = await _controller.RestoreUser(
            1, CancellationToken.None);

        Assert.That(result.Result, Is.TypeOf<OkObjectResult>());
    }

    #endregion
}
