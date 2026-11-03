using CSharpFunctionalExtensions;


public class AuthService(
    IUserRepository _userRepository,
    IJwtService _jwtService,
    ILogger<AuthService> logger
) : IAuthService
{
    // =========================================================
    // CREATE
    // =========================================================
    public async Task<Result<AuthResponse, DomainError>> SignupAsync(SingupRequest request)
    {
        var existing = await _userRepository.GetUserByEmailAsync(request.Email);
        if (existing is not null)
            return Result.Failure<AuthResponse, DomainError>(
                AuthError.DuplicateUser(request.Email)
            );

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 11),
            Role = Role.USER,
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
    public async Task<Result<AuthResponse, DomainError>> LoginAsync(
        LoginRequest request
    )
    {
        var user = await _userRepository.GetUserByEmailAsync(request.Email);
        if (user is null)
            return Result.Failure<AuthResponse, DomainError>(
                    AuthError.InvalidCredentials()
                );
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Result.Failure<AuthResponse, DomainError>(
                AuthError.InvalidCredentials()
            );
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateUserAsync(user.Id, user);

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