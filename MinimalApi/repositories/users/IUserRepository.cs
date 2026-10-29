
/// <summary>
/// Define las operaciones de acceso a datos para la entidad User.
/// Incluye operaciones CRUD, actualización parcial de campos
/// específicos, borrado lógico y restauración.
/// </summary>
public interface IUserRepository
{
    // =========================================================
    // CREATE
    // =========================================================

    /// <summary>
    /// Crea un nuevo usuario en la base de datos.
    /// </summary>
    /// <param name="user">Usuario que se desea crear.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>El usuario creado con su identificador generado.</returns>
    Task<User> CreateUserAsync(
        User user,
        CancellationToken ct = default);

    // =========================================================
    // READ
    // =========================================================

    /// <summary>
    /// Obtiene un usuario por su identificador, excluyendo los
    /// usuarios eliminados lógicamente.
    /// </summary>
    /// <param name="id">Identificador del usuario.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>
    /// El usuario encontrado o null si no existe o está eliminado.
    /// </returns>
    Task<User?> GetUserByIdAsync(
        long id,
        CancellationToken ct = default);
    /// <summary>
    /// Obtiene un usuario por su identificador, excluyendo los
    /// usuarios eliminados lógicamente.
    /// </summary>
    /// <param name="email">Email del usuario.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>
    /// El usuario encontrado o null si no existe o está eliminado.
    /// </returns>
    Task<User?> GetUserByEmailAsync(
        string email,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene todos los usuarios que no han sido eliminados
    /// lógicamente.
    /// </summary>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>Lista de usuarios activos.</returns>
    Task<List<User>> GetAllUsersAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene una página de usuarios activos junto con los
    /// metadatos necesarios para la paginación.
    /// </summary>
    /// <param name="pageNumber">Número de página, comenzando en 1.</param>
    /// <param name="pageSize">Número máximo de usuarios por página.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>
    /// Una tupla que contiene los usuarios de la página,
    /// el total de registros, el total de páginas y los indicadores
    /// de existencia de página anterior y siguiente.
    /// </returns>
    Task<(
        List<User> items,
        int count,
        int totalPages,
        bool HasPreviousPage,
        bool HasNextPage
    )> GetPaginatedUsersAsync(
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    // =========================================================
    // UPDATE
    // =========================================================

    /// <summary>
    /// Actualiza los datos generales de un usuario activo,
    /// sin modificar su contraseña ni su rol.
    /// </summary>
    /// <param name="id">Identificador del usuario que se actualizará.</param>
    /// <param name="user">Usuario con los nuevos datos generales.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>
    /// El usuario actualizado o null si no existe o está eliminado.
    /// </returns>
    Task<User?> UpdateUserAsync(
        long id,
        User user,
        CancellationToken ct = default);

    /// <summary>
    /// Actualiza exclusivamente el hash de la contraseña de un
    /// usuario activo e invalida su token de renovación.
    /// </summary>
    /// <param name="id">Identificador del usuario.</param>
    /// <param name="passwordHash">Nuevo hash de la contraseña.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>
    /// El usuario actualizado o null si no existe o está eliminado.
    /// </returns>
    Task<User?> UpdatePasswordAsync(
        long id,
        string passwordHash,
        CancellationToken ct = default);

    /// <summary>
    /// Actualiza exclusivamente el rol de un usuario activo.
    /// </summary>
    /// <param name="id">Identificador del usuario.</param>
    /// <param name="role">Nuevo rol que se asignará al usuario.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>
    /// El usuario actualizado o null si no existe o está eliminado.
    /// </returns>
    Task<User?> UpdateRoleAsync(
        long id,
        string role,
        CancellationToken ct = default);

    // =========================================================
    // DELETE / RESTORE
    // =========================================================

    /// <summary>
    /// Realiza el borrado lógico de un usuario activo,
    /// marcándolo como eliminado y revocando su token de renovación.
    /// </summary>
    /// <param name="id">Identificador del usuario que se eliminará.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>
    /// true si el usuario se eliminó correctamente;
    /// false si no existe o ya estaba eliminado.
    /// </returns>
    Task<bool> DeleteUserAsync(
        long id,
        CancellationToken ct = default);

    /// <summary>
    /// Restaura un usuario previamente eliminado lógicamente.
    /// </summary>
    /// <param name="id">Identificador del usuario que se restaurará.</param>
    /// <param name="ct">Token para cancelar la operación.</param>
    /// <returns>
    /// El usuario restaurado o null si no existe o no está eliminado.
    /// </returns>
    Task<User?> RestoreUserAsync(
        long id,
        CancellationToken ct = default);
}
