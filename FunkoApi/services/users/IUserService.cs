using CSharpFunctionalExtensions;

/// <summary>
/// Define las operaciones de servicio para la gestión de usuarios.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Elimina lógicamente un usuario.
    /// </summary>
    /// <param name="id">ID del usuario que se desea eliminar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>true si el usuario se eliminó correctamente o un error de dominio.</returns>
    Task<Result<bool, DomainError>> DeleteUserAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los usuarios.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de usuarios o un error de dominio.</returns>
    Task<Result<List<UserDto>, DomainError>> GetAllUsersAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtiene una lista paginada de usuarios.
    /// </summary>
    /// <param name="pageNumber">Número de página solicitada.</param>
    /// <param name="pageSize">Cantidad de usuarios por página.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Usuarios paginados con sus metadatos o un error de dominio.</returns>
    Task<Result<PaginatedResouce<UserDto>, DomainError>> GetPaginatedUsersAsync(int pageNumber, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Obtiene un usuario por su email.
    /// </summary>
    /// <param name="email">Email del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Usuario encontrado o un error de dominio.</returns>
    Task<Result<UserDto, DomainError>> GetUserByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Obtiene un usuario por su ID.
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Usuario encontrado o un error de dominio.</returns>
    Task<Result<UserDto, DomainError>> GetUserByIdAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Restaura un usuario eliminado lógicamente.
    /// </summary>
    /// <param name="id">ID del usuario que se desea restaurar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Usuario restaurado o un error de dominio.</returns>
    Task<Result<UserDto, DomainError>> RestoreUserAsync(long id, CancellationToken ct = default);

    /// <summary>
    /// Actualiza la contraseña de un usuario.
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    /// <param name="passwordHash">Nueva contraseña cifrada.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Usuario actualizado o un error de dominio.</returns>
    Task<Result<UserDto, DomainError>> UpdatePasswordAsync(long id, string passwordHash, CancellationToken ct = default);

    /// <summary>
    /// Actualiza el rol de un usuario.
    /// </summary>
    /// <param name="id">ID del usuario.</param>
    /// <param name="role">Nuevo rol del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Usuario actualizado o un error de dominio.</returns>
    Task<Result<UserDto, DomainError>> UpdateRoleAsync(long id, string role, CancellationToken ct = default);

    /// <summary>
    /// Actualiza los datos de un usuario.
    /// </summary>
    /// <param name="id">ID del usuario que se desea actualizar.</param>
    /// <param name="user">Nuevos datos del usuario.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Usuario actualizado o un error de dominio.</returns>
    Task<Result<UserDto, DomainError>> UpdateUserAsync(long id, UserUpdateRegularRequestDto user, CancellationToken ct = default);
}