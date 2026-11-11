
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

[ApiController]
[Route("api/users")]
public class UserController(
    IUserService _userService
) : ControllerBase
{
    // =========================================================
    // READ
    // =========================================================

    [HttpGet("{id:long}")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "Obtener usuario por ID",
        Description = "Obtiene la información de un usuario."
    )]
    [SwaggerResponse(200, "Usuario recuperado correctamente.", typeof(UserDto))]
    [SwaggerResponse(404, "Usuario no encontrado.")]
    public async Task<ActionResult<UserDto>> GetUserById(
        long id,
        CancellationToken ct)
    {
        var result = await _userService.GetUserByIdAsync(id, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.ToHttpResult<UserDto>();
    }

    [HttpGet("me")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Obtener mi perfil",
        Description = "Obtiene la información del usuario autenticado."
    )]
    [SwaggerResponse(200, "Usuario recuperado correctamente.", typeof(UserDto))]
    [SwaggerResponse(401, "Usuario no autenticado.")]
    [SwaggerResponse(404, "Usuario no encontrado.")]
    public async Task<ActionResult<UserDto>> GetMe(
        CancellationToken ct)
    {
        // Obtener ID desde los claims del JWT
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrEmpty(userId) ||
            !long.TryParse(userId, out var id))
            return Unauthorized();

        var result = await _userService.GetUserByIdAsync(id, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.ToHttpResult<UserDto>();
    }

    [HttpGet("email/{email}")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "Obtener usuario por email",
        Description = "Busca un usuario mediante su email."
    )]
    [SwaggerResponse(200, "Usuario recuperado correctamente.", typeof(UserDto))]
    [SwaggerResponse(404, "Usuario no encontrado.")]
    public async Task<ActionResult<UserDto>> GetUserByEmail(
        string email,
        CancellationToken ct)
    {
        var result = await _userService.GetUserByEmailAsync(email, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.ToHttpResult<UserDto>();
    }

    [HttpGet]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "Obtener todos los usuarios",
        Description = "Recupera todos los usuarios activos."
    )]
    [SwaggerResponse(200, "Usuarios recuperados correctamente.")]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers(
        CancellationToken ct)
    {
        var result = await _userService.GetAllUsersAsync(ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.ToHttpResult<List<UserDto>>();
    }

    [HttpGet("paginated")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "Obtener usuarios paginados",
        Description = "Recupera los usuarios mediante paginación."
    )]
    [SwaggerResponse(200, "Usuarios recuperados correctamente.")]
    public async Task<ActionResult<PaginatedResouce<UserDto>>> GetPaginatedUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await _userService.GetPaginatedUsersAsync(
            pageNumber,
            pageSize,
            ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.ToHttpResult<PaginatedResouce<UserDto>>();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    [HttpPut("{id:long}")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "Actualizar usuario",
        Description = "Actualiza los datos generales de un usuario."
    )]
    [SwaggerResponse(200, "Usuario actualizado correctamente.")]
    [SwaggerResponse(404, "Usuario no encontrado.")]
    public async Task<ActionResult<UserDto>> UpdateUser(
        long id,
        [FromBody] UserUpdateRegularRequestDto user,
        CancellationToken ct)
    {
        var result = await _userService.UpdateUserAsync(id, user, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.ToHttpResult<UserDto>();
    }
    // ===== Actualizar usuario su propia cuenta ========
    [HttpPut("me")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Actualizar mi perfil",
        Description = "Permite al usuario autenticado actualizar sus propios datos."
    )]
    [SwaggerResponse(200, "Usuario actualizado correctamente.", typeof(UserDto))]
    [SwaggerResponse(401, "Usuario no autenticado.")]
    [SwaggerResponse(404, "Usuario no encontrado.")]
    public async Task<ActionResult<UserDto>> UpdateMe(
        [FromBody] UserUpdateRegularRequestDto user,
        CancellationToken ct)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!long.TryParse(userId, out var id))
            return Unauthorized();

        var result = await _userService.UpdateUserAsync(id, user, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.ToHttpResult<UserDto>();
}

    [HttpPatch("{id:long}/password")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Actualizar contraseña",
        Description = "Actualiza la contraseña de un usuario."
    )]
    [SwaggerResponse(200, "Contraseña actualizada correctamente.")]
    [SwaggerResponse(404, "Usuario no encontrado.")]
    public async Task<ActionResult<UserDto>> UpdatePassword(
        long id,
        [FromBody] string passwordHash,
        CancellationToken ct)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!long.TryParse(userId, out var authenticatedId))
            return Unauthorized();

        // Un usuario normal solo puede modificar su contraseña.
        if (authenticatedId != id && !User.IsInRole("ADMIN"))
            return Forbid();

        var result = await _userService.UpdatePasswordAsync(
            id,
            passwordHash,
            ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.ToHttpResult<UserDto>();
    }

    [HttpPatch("{id:long}/role")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "Actualizar rol",
        Description = "Modifica el rol de un usuario."
    )]
    [SwaggerResponse(200, "Rol actualizado correctamente.")]
    [SwaggerResponse(404, "Usuario no encontrado.")]
    public async Task<ActionResult<UserDto>> UpdateRole(
        long id,
        [FromBody] string role,
        CancellationToken ct)
    {
        var result = await _userService.UpdateRoleAsync(id, role, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.ToHttpResult<UserDto>();
    }

    // =========================================================
    // DELETE
    // =========================================================

    [HttpDelete("{id:long}")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "Eliminar usuario",
        Description = "Realiza un borrado lógico del usuario."
    )]
    [SwaggerResponse(204, "Usuario eliminado correctamente.")]
    [SwaggerResponse(404, "Usuario no encontrado.")]
    public async Task<ActionResult<object>> DeleteUser(
        long id,
        CancellationToken ct)
    {
        var result = await _userService.DeleteUserAsync(id, ct);

        return result.IsSuccess
            ? NoContent()
            : result.Error.ToHttpResult<object>();
    }

    // ======= Eliminarme =======
    [HttpDelete("me")]
    [Authorize(Roles = "USER")] //
    [SwaggerOperation(
        Summary = "Eliminar mi cuenta",
        Description = "Permite al usuario autenticado eliminar su propia cuenta."
    )]
    [SwaggerResponse(204, "Cuenta eliminada correctamente.")]
    [SwaggerResponse(401, "Usuario no autenticado.")]
    [SwaggerResponse(404, "Usuario no encontrado.")]
    public async Task<ActionResult<bool>> DeleteMe(
        CancellationToken ct)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!long.TryParse(userId, out var id))
            return Unauthorized();

        var result = await _userService.DeleteUserAsync(id, ct);

        return result.IsSuccess
            ? NoContent()
            : result.Error.ToHttpResult<bool>();
    }

    // =========================================================
    // RESTORE
    // =========================================================

    [HttpPatch("{id:long}/restore")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "Restaurar usuario",
        Description = "Restaura un usuario eliminado lógicamente."
    )]
    [SwaggerResponse(200, "Usuario restaurado correctamente.")]
    [SwaggerResponse(404, "Usuario no encontrado.")]
    public async Task<ActionResult<UserDto>> RestoreUser(
        long id,
        CancellationToken ct)
    {
        var result = await _userService.RestoreUserAsync(id, ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.ToHttpResult<UserDto>();
    }
}
