using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

public static class AuthenticationConfig
{
    public static IServiceCollection AddAuthenticationJwt(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var secretKey = jwtSection["Secret"]
            ?? throw new InvalidOperationException("JWT Secret no configurado");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // Sin mapeo inbound: los claims conservan sus tipos originales
            // ("sub", "email", "role"), necesarios para User.FindFirst("sub")
            // y para que RequireRole lea RoleClaimType = "role".
            options.MapInboundClaims = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtSection["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSection["Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(secretKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                // El claim "role" del JWT se usa como rol para RequireRole/IsInRole
                RoleClaimType = "role"
            };

            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();
                    logger.LogDebug("Token validado para: {User}",
                        context.Principal?.Identity?.Name);
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>();
                    logger.LogWarning("Fallo de autenticacion: {Message}",
                        context.Exception.Message);
                    return Task.CompletedTask;
                }
            };
        });

        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", policy =>
                policy.RequireRole(Role.ADMIN.ToString()))  // lee RoleClaimType = "role"
            .AddPolicy("UserOrAdmin", policy =>
                policy.RequireRole(Role.USER.ToString(), Role.ADMIN.ToString()));

        services.AddScoped<JwtService>();
        services.AddScoped<AuthService>();

        return services;
    }
}