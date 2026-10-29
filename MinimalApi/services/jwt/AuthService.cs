public class AuthService(
    IUserRepository _userRepository,
    JwtService _jwtService,
    ILogger<AuthService> logger
)
{
    public async Task<AuthResponse> SignupAsync(SingupRequest request)
    {
        var existing = await _userRepository.GetUserByEmailAsync(request.Email);
        if(existing is not null)
            throw new InvalidOperationException("El email ya está registrado");
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 11),
            Role = Role.USER,
            CreatedAt = DateTime.UtcNow
        };
        user = await _userRepository.CreateUserAsync(user);
        return new AuthResponse(
            _jwtService.GenerateToken(user),
            _jwtService.GenerateRefreshToken(),
            "Bearer",
            _jwtService.AccessTokenExpirySeconds,
            user.Id,
            user.Username,
            user.Role.ToString());
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request
    )
    {
        var user = await _userRepository.GetUserByEmailAsync(request.Email) ?? throw new UnauthorizedAccessException("Credenciales invalidas");
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            logger.LogWarning("Intento de login fallido para: {Email}", request.Email);
            throw new UnauthorizedAccessException("Credenciales invalidas");
        }
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateUserAsync(user.Id, user);

        return new AuthResponse(
            _jwtService.GenerateToken(user),
            _jwtService.GenerateRefreshToken(),
            "Bearer",
            _jwtService.AccessTokenExpirySeconds,
            user.Id,
            user.Username,
            user.Role.ToString());
        
    }
}