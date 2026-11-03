using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProductosApi.Controllers;

/// <summary>
/// Controller de autenticacion: login, signup y perfil de usuario.
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Registra un usuario nuevo.
    /// </summary>
    [HttpPost("signup")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Signup([FromBody] SingupRequest request)
    {
        var result = await authService.SignupAsync(request);
        return result.IsSuccess ?
            Ok(result.Value) :
            result.Error.ToHttpResult<AuthResponse>();
    }

    /// <summary>
    /// Inicia sesion y devuelve un JWT token.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        return result.IsSuccess ?
            Ok(result.Value) :
            result.Error.ToHttpResult<AuthResponse>();
    }

}