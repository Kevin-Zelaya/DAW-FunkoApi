
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Testcontainers.PostgreSql;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Testcontainers.PostgreSql;

[TestFixture]
public class UserRepositoryTests
{
    private PostgreSqlContainer _container = null!;
    private AppDbContext _context = null!;
    private UserRepository _repository = null!;
    private Mock<ILogger<UserRepository>> _logger = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithDatabase("user_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

        await _container.StartAsync();
    }

    [SetUp]
    public async Task Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        _context = new AppDbContext(options);

        await _context.Database.EnsureDeletedAsync();
        await _context.Database.EnsureCreatedAsync();

        _logger = new Mock<ILogger<UserRepository>>();
        _repository = new UserRepository(_context, _logger.Object);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _context.DisposeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _container.DisposeAsync();
    }

    public User CreateUserValid() => new User
    {
        Username = $"usuario-{Guid.NewGuid():N}",
        Email = $"usuario-{Guid.NewGuid():N}@test.com",
        PasswordHash = "PasswordHashTest",
        Role = Enum.GetValues<Role>()[0],
        IsDeleted = false
    };

    // =========================================================
    // CREATE
    // =========================================================

    [Test]
    public async Task CreateUser_ValidUser_UserCreated()
    {
        // Arrange
        var user = CreateUserValid();

        // Act
        var created = await _repository.CreateUserAsync(user);

        // Assert
        Assert.That(created.Id, Is.GreaterThan(0));
        Assert.That(created.Username, Is.EqualTo(user.Username));
        Assert.That(created.Email, Is.EqualTo(user.Email));
        Assert.That(created.IsDeleted, Is.False);
        Assert.That(created.CreatedAt, Is.Not.EqualTo(default(DateTime)));
        Assert.That(created.LastLoginAt, Is.Null);
        Assert.That(created.RefreshToken, Is.Null);
        Assert.That(created.RefreshTokenExpiry, Is.Null);
    }

    [Test]
    public async Task CreateUser_ExistingUser_UserPersisted()
    {
        // Arrange
        var user = CreateUserValid();

        // Act
        await _repository.CreateUserAsync(user);

        var response = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == user.Email);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Username, Is.EqualTo(user.Username));
    }

    // =========================================================
    // READ
    // =========================================================

    [Test]
    public async Task GetUserById_ExistingUser_UserReturned()
    {
        // Arrange
        var user = CreateUserValid();
        await _repository.CreateUserAsync(user);

        // Act
        var response = await _repository.GetUserByIdAsync(user.Id);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Id, Is.EqualTo(user.Id));
        Assert.That(response.Email, Is.EqualTo(user.Email));
    }

    [Test]
    public async Task GetUserById_NonExistingUser_NullReturned()
    {
        // Act
        var response = await _repository.GetUserByIdAsync(999);

        // Assert
        Assert.That(response, Is.Null);
    }

    [Test]
    public async Task GetUserById_DeletedUser_NullReturned()
    {
        // Arrange
        var user = CreateUserValid();
        await _repository.CreateUserAsync(user);

        await _repository.DeleteUserAsync(user.Id);

        // Act
        var response = await _repository.GetUserByIdAsync(user.Id);

        // Assert
        Assert.That(response, Is.Null);
    }

    [Test]
    public async Task GetUserByEmail_ExistingUser_UserReturned()
    {
        // Arrange
        var user = CreateUserValid();
        await _repository.CreateUserAsync(user);

        // Act
        var response = await _repository.GetUserByEmailAsync(user.Email);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Email, Is.EqualTo(user.Email));
    }

    [Test]
    public async Task GetUserByEmail_NonExistingEmail_NullReturned()
    {
        // Act
        var response = await _repository.GetUserByEmailAsync(
            "inexistente@test.com"
        );

        // Assert
        Assert.That(response, Is.Null);
    }

    [Test]
    public async Task GetUserByEmail_DeletedUser_NullReturned()
    {
        // Arrange
        var user = CreateUserValid();
        await _repository.CreateUserAsync(user);

        await _repository.DeleteUserAsync(user.Id);

        // Act
        var response = await _repository.GetUserByEmailAsync(user.Email);

        // Assert
        Assert.That(response, Is.Null);
    }

    [Test]
    public async Task GetAllUsers_ExistingUsers_UsersReturned()
    {
        // Arrange
        var user1 = CreateUserValid();
        var user2 = CreateUserValid();

        await _repository.CreateUserAsync(user1);
        await _repository.CreateUserAsync(user2);

        // Act
        var response = await _repository.GetAllUsersAsync();

        // Assert
        // 1 usuario del seed + 2 usuarios creados.
        Assert.That(response, Has.Count.EqualTo(4));
        Assert.That(response.Any(u => u.Id == user1.Id), Is.True);
        Assert.That(response.Any(u => u.Id == user2.Id), Is.True);
    }

    [Test]
    public async Task GetAllUsers_SeededUser_UserReturned()
    {
        // Act
        var response = await _repository.GetAllUsersAsync();

        // Assert
        // La base de datos contiene un usuario inicial.
        Assert.That(response, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task GetAllUsers_DeletedUser_DeletedUserNotReturned()
    {
        // Arrange
        var user1 = CreateUserValid();
        var user2 = CreateUserValid();

        await _repository.CreateUserAsync(user1);
        await _repository.CreateUserAsync(user2);

        await _repository.DeleteUserAsync(user1.Id);

        // Act
        var response = await _repository.GetAllUsersAsync();

        // Assert
        // 1 usuario del seed + 1 usuario activo.
        Assert.That(response, Has.Count.EqualTo(3));
        Assert.That(response.Any(u => u.Id == user1.Id), Is.False);
        Assert.That(response.Any(u => u.Id == user2.Id), Is.True);
    }

    // =========================================================
    // PAGINATION
    // =========================================================

    [Test]
    public async Task GetPaginatedUsers_FirstPage_CorrectPagination()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
            await _repository.CreateUserAsync(CreateUserValid());

        // Act
        var response = await _repository.GetPaginatedUsersAsync(1, 2);

        // Assert
        // 1 usuario del seed + 5 creados = 6.
        Assert.That(response.items, Has.Count.EqualTo(2));
        Assert.That(response.count, Is.EqualTo(7));
        Assert.That(response.totalPages, Is.EqualTo(4));
        Assert.That(response.HasPreviousPage, Is.False);
        Assert.That(response.HasNextPage, Is.True);
    }

    [Test]
    public async Task GetPaginatedUsers_IntermediatePage_CorrectPagination()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
            await _repository.CreateUserAsync(CreateUserValid());

        // Act
        var response = await _repository.GetPaginatedUsersAsync(2, 2);

        // Assert
        Assert.That(response.items, Has.Count.EqualTo(2));
        Assert.That(response.count, Is.EqualTo(7));
        Assert.That(response.totalPages, Is.EqualTo(4));
        Assert.That(response.HasPreviousPage, Is.True);
        Assert.That(response.HasNextPage, Is.True);
    }

    [Test]
    public async Task GetPaginatedUsers_LastPage_CorrectPagination()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
            await _repository.CreateUserAsync(CreateUserValid());

        // Act
        var response = await _repository.GetPaginatedUsersAsync(3, 2);

        // Assert
        // 6 usuarios / 2 por página = 3 páginas completas.
        Assert.That(response.items, Has.Count.EqualTo(2));
        Assert.That(response.count, Is.EqualTo(7));
        Assert.That(response.totalPages, Is.EqualTo(4));
        Assert.That(response.HasPreviousPage, Is.True);
    }

    [Test]
    public async Task GetPaginatedUsers_SeededUser_UserReturned()
    {
        // Act
        var response = await _repository.GetPaginatedUsersAsync(1, 2);

        // Assert
        Assert.That(response.items, Has.Count.EqualTo(2));
        Assert.That(response.count, Is.EqualTo(2));
        Assert.That(response.totalPages, Is.EqualTo(1));
        Assert.That(response.HasPreviousPage, Is.False);
        Assert.That(response.HasNextPage, Is.False);
    }

    [Test]
    public void GetPaginatedUsers_InvalidPageNumber_ArgumentOutOfRangeException()
    {
        // Assert
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await _repository.GetPaginatedUsersAsync(0, 2)
        );
    }

    [Test]
    public void GetPaginatedUsers_InvalidPageSize_ArgumentOutOfRangeException()
    {
        // Assert
        Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await _repository.GetPaginatedUsersAsync(1, 0)
        );
    }

    // =========================================================
    // EXIST
    // =========================================================

    [Test]
    public async Task ExistByUsername_ExistingUsername_TrueReturned()
    {
        // Arrange
        var user = CreateUserValid();
        await _repository.CreateUserAsync(user);

        // Act
        var response = await _repository.ExistByUsername(user.Username);

        // Assert
        Assert.That(response, Is.True);
    }

    [Test]
    public async Task ExistByUsername_NonExistingUsername_FalseReturned()
    {
        // Act
        var response = await _repository.ExistByUsername(
            "usuario_inexistente"
        );

        // Assert
        Assert.That(response, Is.False);
    }

    [Test]
    public async Task ExistByUsername_DeletedUser_TrueReturned()
    {
        // Arrange
        var user = CreateUserValid();
        await _repository.CreateUserAsync(user);

        await _repository.DeleteUserAsync(user.Id);

        // Act
        var response = await _repository.ExistByUsername(user.Username);

        // Assert
        // El usuario sigue existiendo físicamente, aunque esté eliminado.
        Assert.That(response, Is.True);
    }

    // =========================================================
    // UPDATE
    // =========================================================

    [Test]
    public async Task UpdateUser_ExistingUser_UserUpdated()
    {
        // Arrange
        var user = CreateUserValid();
        await _repository.CreateUserAsync(user);

        var updated = new UserUpdateRegularRequestDto(
            user.Id,
            "usuario_actualizado",
            "actualizado@test.com"
        );

        // Act
        var response = await _repository.UpdateUserAsync(user.Id, updated);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Username, Is.EqualTo(updated.Username));
        Assert.That(response.Email, Is.EqualTo(updated.Email));
    }

    [Test]
    public async Task UpdateUser_NonExistingUser_NullReturned()
    {
        // Arrange
        var updated = new UserUpdateRegularRequestDto(
            999,
            "usuario_actualizado",
            "actualizado@test.com"
        );

        // Act
        var response = await _repository.UpdateUserAsync(999, updated);

        // Assert
        Assert.That(response, Is.Null);
    }

    [Test]
    public async Task UpdateUser_DeletedUser_NullReturned()
    {
        // Arrange
        var user = CreateUserValid();
        await _repository.CreateUserAsync(user);

        await _repository.DeleteUserAsync(user.Id);

        var updated = new UserUpdateRegularRequestDto(
            user.Id,
            "usuario_actualizado",
            "actualizado@test.com"
        );

        // Act
        var response = await _repository.UpdateUserAsync(user.Id, updated);

        // Assert
        Assert.That(response, Is.Null);
    }

    [Test]
    public async Task UpdatePassword_ExistingUser_PasswordUpdated()
    {
        // Arrange
        var user = CreateUserValid();
        user.RefreshToken = "refresh-token-test";
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        await _repository.CreateUserAsync(user);

        // Act
        var response = await _repository.UpdatePasswordAsync(
            user.Id,
            "new-password-hash"
        );

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.PasswordHash, Is.EqualTo("new-password-hash"));
        Assert.That(response.RefreshToken, Is.Null);
        Assert.That(response.RefreshTokenExpiry, Is.Null);
    }

    [Test]
    public async Task UpdatePassword_NonExistingUser_NullReturned()
    {
        // Act
        var response = await _repository.UpdatePasswordAsync(
            999,
            "new-password-hash"
        );

        // Assert
        Assert.That(response, Is.Null);
    }

    [Test]
    public async Task UpdateRole_ExistingUser_RoleUpdated()
    {
        // Arrange
        var user = CreateUserValid();
        await _repository.CreateUserAsync(user);

        var roles = Enum.GetValues<Role>();
        Assert.That(roles.Length, Is.GreaterThan(1));

        var newRole = roles.First(role => !role.Equals(user.Role));

        // Act
        var response = await _repository.UpdateRoleAsync(
            user.Id,
            newRole.ToString()
        );

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Role, Is.EqualTo(newRole));
    }

    [Test]
    public async Task UpdateRole_NonExistingUser_NullReturned()
    {
        // Arrange
        var role = Enum.GetValues<Role>()[0].ToString();

        // Act
        var response = await _repository.UpdateRoleAsync(999, role);

        // Assert
        Assert.That(response, Is.Null);
    }

    [Test]
    public async Task UpdateRole_InvalidRole_BadHttpRequestException()
    {
        // Arrange
        var user = CreateUserValid();
        await _repository.CreateUserAsync(user);

        // Act
        Assert.ThrowsAsync<BadHttpRequestException>(
            async () => await _repository.UpdateRoleAsync(
                user.Id,
                "RolInexistente"
            )
        );
    }

    [Test]
    public async Task UpdateRole_DeletedUser_NullReturned()
    {
        // Arrange
        var user = CreateUserValid();
        await _repository.CreateUserAsync(user);

        await _repository.DeleteUserAsync(user.Id);

        var role = Enum.GetValues<Role>()[0].ToString();

        // Act
        var response = await _repository.UpdateRoleAsync(user.Id, role);

        // Assert
        Assert.That(response, Is.Null);
    }

    // =========================================================
    // DELETE
    // =========================================================

    [Test]
    public async Task DeleteUser_ExistingUser_TrueReturned()
    {
        // Arrange
        var user = CreateUserValid();
        await _repository.CreateUserAsync(user);

        // Act
        var response = await _repository.DeleteUserAsync(user.Id);

        // Assert
        Assert.That(response, Is.True);

        var deleted = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        Assert.That(deleted, Is.Not.Null);
        Assert.That(deleted!.IsDeleted, Is.True);
        Assert.That(deleted.RefreshToken, Is.Null);
        Assert.That(deleted.RefreshTokenExpiry, Is.Null);
    }

    [Test]
    public async Task DeleteUser_NonExistingUser_FalseReturned()
    {
        // Act
        var response = await _repository.DeleteUserAsync(999);

        // Assert
        Assert.That(response, Is.False);
    }

    [Test]
    public async Task DeleteUser_AlreadyDeletedUser_FalseReturned()
    {
        // Arrange
        var user = CreateUserValid();
        await _repository.CreateUserAsync(user);

        await _repository.DeleteUserAsync(user.Id);

        // Act
        var response = await _repository.DeleteUserAsync(user.Id);

        // Assert
        Assert.That(response, Is.False);
    }

    // =========================================================
    // RESTORE
    // =========================================================

    [Test]
    public async Task RestoreUser_DeletedUser_UserRestored()
    {
        // Arrange
        var user = CreateUserValid();
        await _repository.CreateUserAsync(user);

        await _repository.DeleteUserAsync(user.Id);

        // Act
        var response = await _repository.RestoreUserAsync(user.Id);

        // Assert
        Assert.That(response, Is.Not.Null);
        Assert.That(response!.IsDeleted, Is.False);
    }

    [Test]
    public async Task RestoreUser_NonExistingUser_NullReturned()
    {
        // Act
        var response = await _repository.RestoreUserAsync(999);

        // Assert
        Assert.That(response, Is.Null);
    }

    [Test]
    public async Task RestoreUser_ActiveUser_NullReturned()
    {
        // Arrange
        var user = CreateUserValid();
        await _repository.CreateUserAsync(user);

        // Act
        var response = await _repository.RestoreUserAsync(user.Id);

        // Assert
        Assert.That(response, Is.Null);
    }
}
