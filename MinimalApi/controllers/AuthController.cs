using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ProductosApi.Controllers;

/// <summary>
/// Controller de autenticacion: login, signup y perfil de usuario.
/// </summary>
/// <summary>
/// Controller de autenticación: registro e inicio de sesión.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    // =========================================================
    // SIGNUP
    // =========================================================

    [HttpPost("signup")]
    [SwaggerOperation(
        Summary = "Registrar un usuario",
        Description = "Registra un nuevo usuario en la aplicación."
    )]
    [SwaggerResponse(201, "Usuario registrado correctamente.", typeof(AuthResponse))]
    [SwaggerResponse(400, "Los datos proporcionados no son válidos.")]
    [SwaggerResponse(409, "El usuario ya existe.")]
    public async Task<ActionResult<AuthResponse>> Signup(
        [FromBody] SingupRequest request)
    {
        var result = await authService.SignupAsync(request);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : result.Error.ToHttpResult<AuthResponse>();
    }

    // =========================================================
    // LOGIN
    // =========================================================

    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Iniciar sesión",
        Description = "Autentica a un usuario y devuelve un token JWT para acceder a los endpoints protegidos."
    )]
    [SwaggerResponse(200, "Inicio de sesión correcto. Se devuelve el token JWT.", typeof(AuthResponse))]
    [SwaggerResponse(400, "Los datos proporcionados no son válidos.")]
    [SwaggerResponse(401, "Credenciales incorrectas.")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.ToHttpResult<AuthResponse>();
    }
}