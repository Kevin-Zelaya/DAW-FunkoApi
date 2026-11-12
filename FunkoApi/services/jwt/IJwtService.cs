using System.Security.Claims;

public interface IJwtService
{
    int AccessTokenExpirySeconds { get; }

    string GenerateRefreshToken();
    string GenerateToken(User user);
    ClaimsPrincipal? validateToken(string token);
}