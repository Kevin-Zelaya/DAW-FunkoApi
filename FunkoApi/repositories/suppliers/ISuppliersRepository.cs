public interface ISupplierRepository
{
    // =========================================================
    // CREATE
    // =========================================================

    /// <summary>
    /// Crea un nuevo proveedor.
    /// </summary>
    /// <param name="supplier">Datos del proveedor que se desea crear.</param>
    /// <param name="ct">Token de cancelación de la operación.</param>
    /// <returns>El proveedor creado.</returns>
    Task<Supplier> CreateSupplierAsync(
        SupplierCreateRequestDto supplier,
        CancellationToken ct = default);
    
    // =========================================================
    // CREATE
    // =========================================================

    /// <summary>
    /// Obtiene todos los proveedores.
    /// </summary>
    /// <param name="ct">Token de cancelación de la operación.</param>
    /// <returns>Lista con todos los proveedores.</returns>
    Task<List<Supplier>> GetAllSuppliersAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene una lista paginada de proveedores.
    /// </summary>
    /// <param name="pageNumber">Número de la página solicitada.</param>
    /// <param name="pageSize">Cantidad de proveedores por página.</param>
    /// <param name="ct">Token de cancelación de la operación.</param>
    /// <returns>Tupla con los proveedores de la página, el total de proveedores, el número de páginas y los indicadores de página anterior y siguiente.</returns>
    Task<(
        List<Supplier> items,
        int count,
        int totalPages,
        bool HasPreviousPage,
        bool HasNextPage
    )> GetPaginatedSuppliersAsync(
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene un proveedor por su identificador.
    /// </summary>
    /// <param name="id">Identificador del proveedor.</param>
    /// <param name="ct">Token de cancelación de la operación.</param>
    /// <returns>El proveedor encontrado o null si no existe.</returns>
    Task<Supplier?> GetSupplierByIdAsync(
        string id,
        CancellationToken ct = default);
    /// <summary>
    /// Comprobar si existe por nombre
    /// </summary>
    /// <param name="name">Nombre a comprobar.</param>
    /// <param name="ct">Token de cancelación de la operación.</param>
    /// <returns>El proveedor encontrado o null si no existe.</returns>
    public Task<bool> ExistByNameAsync(
        string name,
        CancellationToken ct = default);

    // =========================================================
    // UPDATE
    // =========================================================

    /// <summary>
    /// Actualiza los datos de un proveedor existente.
    /// </summary>
    /// <param name="id">Identificador del proveedor que se desea actualizar.</param>
    /// <param name="request">Nuevos datos del proveedor, incluida la versión de concurrencia.</param>
    /// <param name="ct">Token de cancelación de la operación.</param>
    /// <returns>El proveedor actualizado o null si no existe.</returns>
    Task<Supplier?> UpdateSupplierAsync(
        string id,
        SupplierUpdateRequestDto request,
        CancellationToken ct = default);

    /// <summary>
    /// Marca un proveedor como eliminado lógicamente.
    /// </summary>
    /// <param name="id">Identificador del proveedor que se desea eliminar.</param>
    /// <param name="ct">Token de cancelación de la operación.</param>
    /// <returns>true si se elimina correctamente; false si no existe.</returns>
    
    // =========================================================
    // UPDATE
    // =========================================================
    Task<bool> DeleteSupplierAsync(
        string id,
        CancellationToken ct = default);

    /// <summary>
    /// Restaura un proveedor eliminado lógicamente.
    /// </summary>
    /// <param name="id">Identificador del proveedor que se desea restaurar.</param>
    /// <param name="ct">Token de cancelación de la operación.</param>
    /// <returns>true si se restaura correctamente; false si no existe.</returns>
    public Task<bool> RestoreSupplierAsync(
        string id,
        CancellationToken ct);
}