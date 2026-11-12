using CSharpFunctionalExtensions;

public interface ISupplierService
{
    // =========================================================
    // CREATE
    // =========================================================

    /// <summary>
    /// Crea un nuevo proveedor.
    /// </summary>
    /// <param name="request">Datos del proveedor que se desea crear.</param>
    /// <param name="ct">Token de cancelación de la operación.</param>
    /// <returns>El proveedor creado o un error de dominio.</returns>
    Task<Result<SupplierDto, DomainError>> CreateSupplierAsync(
        SupplierCreateRequestDto request,
        CancellationToken ct = default);

    // =========================================================
    // READ
    // =========================================================

    /// <summary>
    /// Obtiene todos los proveedores.
    /// </summary>
    /// <param name="ct">Token de cancelación de la operación.</param>
    /// <returns>Lista de proveedores o un error de dominio.</returns>
    Task<Result<List<SupplierDto>, DomainError>> GetAllSuppliersAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene los proveedores de una página.
    /// </summary>
    /// <param name="pageNumber">Número de la página solicitada.</param>
    /// <param name="pageSize">Cantidad de proveedores por página.</param>
    /// <param name="ct">Token de cancelación de la operación.</param>
    /// <returns>Proveedores paginados con sus metadatos o un error de dominio.</returns>
    Task<Result<PaginatedResouce<SupplierDto>, DomainError>> GetPaginatedSuppliersAsync(
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>
    /// Obtiene un proveedor por su identificador.
    /// </summary>
    /// <param name="id">Identificador del proveedor.</param>
    /// <param name="ct">Token de cancelación de la operación.</param>
    /// <returns>El proveedor encontrado o un error de dominio.</returns>
    Task<Result<SupplierDto, DomainError>> GetSupplierByIdAsync(
        string id,
        CancellationToken ct = default);

    // =========================================================
    // UPDATE
    // =========================================================

    /// <summary>
    /// Actualiza los datos de un proveedor existente.
    /// </summary>
    /// <param name="id">Identificador del proveedor que se desea actualizar.</param>
    /// <param name="request">Nuevos datos del proveedor.</param>
    /// <param name="ct">Token de cancelación de la operación.</param>
    /// <returns>El proveedor actualizado o un error de dominio.</returns>
    Task<Result<SupplierDto, DomainError>> UpdateSupplierAsync(
        string id,
        SupplierUpdateRequestDto request,
        CancellationToken ct = default);

    // =========================================================
    // DELETE
    // =========================================================

    /// <summary>
    /// Elimina lógicamente un proveedor.
    /// </summary>
    /// <param name="id">Identificador del proveedor que se desea eliminar.</param>
    /// <param name="ct">Token de cancelación de la operación.</param>
    /// <returns>true si se elimina correctamente o un error de dominio.</returns>
    Task<Result<bool, DomainError>> DeleteSupplierAsync(
        string id,
        CancellationToken ct = default);

    // =========================================================
    // RESTORE
    // =========================================================

    /// <summary>
    /// Restaura un proveedor eliminado lógicamente.
    /// </summary>
    /// <param name="id">Identificador del proveedor que se desea restaurar.</param>
    /// <param name="ct">Token de cancelación de la operación.</param>
    /// <returns>true si se restaura correctamente o un error de dominio.</returns>
    Task<Result<bool, DomainError>> RestoreSupplierAsync(
        string id,
        CancellationToken ct = default);
}