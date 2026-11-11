using CSharpFunctionalExtensions;

/// <summary>
/// Define las operaciones de servicio para la autenticación de usuarios.
/// </summary>
public class AuthService(
    IUserRepository _userRepository,
    IJwtService _jwtService
) : IAuthService
{
    // =========================================================
    // CREATE
    // =========================================================

    /// <inheritdoc cref="IAuthService.SignupAsync(SingupRequest)"/>
    public async Task<Result<AuthResponse, DomainError>> SignupAsync(
        SingupRequest request)
    {
        return await RegisterAsync(request, Role.USER);
    }

    /// <inheritdoc cref="IAuthService.SignupAdminAsync(SingupRequest)"/>
    public async Task<Result<AuthResponse, DomainError>> SignupAdminAsync(
        SingupRequest request)
    {
        return await RegisterAsync(request, Role.ADMIN);
    }

    /// <summary>
    /// Registra un usuario con el rol indicado.
    /// </summary>
    /// <param name="request">Datos necesarios para registrar al usuario.</param>
    /// <param name="role">Rol que se asignará al usuario.</param>
    /// <returns>Datos de autenticación o un error de dominio.</returns>
    private async Task<Result<AuthResponse, DomainError>> RegisterAsync(
        SingupRequest request,
        Role role)
    {
        var existing = await _userRepository.GetUserByEmailAsync(
            request.Email);

        if (existing is not null)
            return Result.Failure<AuthResponse, DomainError>(
                AuthError.DuplicateUser(request.Email));

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(
                request.Password,
                workFactor: 11),
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        user = await _userRepository.CreateUserAsync(user);

        var toReturn = new AuthResponse(
            _jwtService.GenerateToken(user),
            _jwtService.GenerateRefreshToken(),
            "Bearer",
            _jwtService.AccessTokenExpirySeconds,
            user.Id,
            user.Username,
            user.Role.ToString());

        return Result.Success<AuthResponse, DomainError>(toReturn);
    }

    // =========================================================
    // READ
    // =========================================================

    /// <inheritdoc cref="IAuthService.LoginAsync(LoginRequest)"/>
    public async Task<Result<AuthResponse, DomainError>> LoginAsync(
        LoginRequest request)
    {
        var user = await _userRepository.GetUserByEmailAsync(
            request.Email);

        if (user is null)
            return Result.Failure<AuthResponse, DomainError>(
                AuthError.InvalidCredentials());

        if (!BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash))
            return Result.Failure<AuthResponse, DomainError>(
                AuthError.InvalidCredentials());

        user.LastLoginAt = DateTime.UtcNow;

        var toReturn = new AuthResponse(
            _jwtService.GenerateToken(user),
            _jwtService.GenerateRefreshToken(),
            "Bearer",
            _jwtService.AccessTokenExpirySeconds,
            user.Id,
            user.Username,
            user.Role.ToString());

        return Result.Success<AuthResponse, DomainError>(toReturn);
    }
}