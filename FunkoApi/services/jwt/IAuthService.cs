using CSharpFunctionalExtensions;

/// <summary>
/// Define las operaciones de servicio para la autenticación de usuarios.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Inicia sesión de un usuario.
    /// </summary>
    /// <param name="request">Datos de acceso del usuario.</param>
    /// <returns>Datos de autenticación o un error de dominio.</returns>
    Task<Result<AuthResponse, DomainError>> LoginAsync(LoginRequest request);

    /// <summary>
    /// Registra un nuevo usuario.
    /// </summary>
    /// <param name="request">Datos necesarios para registrar al usuario.</param>
    /// <returns>Datos de autenticación o un error de dominio.</returns>
    Task<Result<AuthResponse, DomainError>> SignupAsync(SingupRequest request);

    /// <summary>
    /// Registra un nuevo administrador.
    /// </summary>
    /// <param name="request">Datos necesarios para registrar al administrador.</param>
    /// <returns>Datos de autenticación o un error de dominio.</returns>
    Task<Result<AuthResponse, DomainError>> SignupAdminAsync(SingupRequest request);
}