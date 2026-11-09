public interface IFunkoRepository
{
    // =========================================================
    // CREATE
    // =========================================================

    /// <summary>
    /// Crear un funko de forma asincrona
    /// </summary>
    /// <param name="funko">Modelo funko con propiedades a crear</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Funko creado</returns>
    Task<Funko> CreateFunkoAsync(Funko funko, CancellationToken ct = default);
    /// <summary>
    /// Eliminar de forma lógica un funko
    /// se le agrega el tiempo de eliminación
    /// </summary>
    /// <param name="id">Id del funkko a eliminar</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>True o false dependiendo de si se encontró o no</returns>
    
    // =========================================================
    // READ
    // =========================================================

    Task<List<Funko>> GetAllFunkosAsync(CancellationToken ct = default);
    /// <summary>
    /// Obtener un funko por id
    /// </summary>
    /// <param name="id">id del usuario a buscar</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Funko encontrado o null</returns>
    Task<Funko?> GetFunkoByIdAsync(long id, CancellationToken ct = default);
    /// <summary>
    /// Comprobar si existe alguno con el username
    /// </summary>
    /// <param name="name">Nombre a comprobar</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Funko encontrado o null</returns>
    public Task<bool> ExistByNameAsync(string name, CancellationToken ct = default);
    /// <summary>
    /// Obtener lista páginada de funkos
    /// </summary>
    /// <param name="pageNumber">Número de página</param>
    /// <param name="pageSize">Tamaño de página</param>
    /// <param name="ct">token de cancelación</param>
    /// <returns>Lista paginada de usuarios</returns>
    Task<(List<Funko> items, int count, int totalPages, bool HasPreviousPage, bool HasNextPage)> GetPaginatedFunkosAsync(int pageNumber, int pageSize, CancellationToken ct);
    /// <summary>
    /// Restablecer funko eliminado de forma lógica
    /// </summary>
    /// <param name="id">Id del funko a restablecer</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>True o false dependiendo de si se encontró</returns>
    
    // =========================================================
    // UPDATE
    // =========================================================

    /// <summary>
    /// Actualizar funko
    /// </summary>
    /// <param name="id">Id del funko a actualizar</param>
    /// <param name="funko">Data del funko</param>
    /// <param name="ct">Token a cancelación</param>
    /// <returns>Data actualiada</returns>
    Task<Funko?> UpdateFunkoAsync(long id, FunkoUpdateRequestDto funko, CancellationToken ct = default);
    /// <summary>
    /// Auxiliar para obtener todas las categorias
    /// </summary>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Lista de categorias</returns>
    
    public  Task<Funko?> UpdateCategoryFromFunkoAsync(long id, FunkoUpdateCategoryRequestDto request, CancellationToken ct = default);
    /// <summary>
    /// Actualizacion parcial, solo
    /// referencia a proveedor
    /// </summary>
    /// <param name="id">Id del funko a actualizar</param>
    /// <param name="SupplierId">Id del proveedor</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Funko actualizado</returns>
    public Task<Funko?> UpdateSuplierFromFunkoAsync(long id, FunkoUpdateSupplierDto request, CancellationToken ct = default);
    /// <summary>
    /// Actualizacion parcial, solo
    /// imagen a proveedor
    /// </summary>
    /// <param name="id">Id del funko a actualizar</param>
    /// <param name="SupplierId">Id del proveedor</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Funko actualizado</returns>
    public Task<bool> UpdateImageFromFunkoAsync(long id, string image, CancellationToken ct = default);
    /// <summary>
    /// Actualizacion parcial, solo
    /// precio
    /// </summary>
    /// <param name="id">Id del funko a actualizar</param>
    /// <param name="SupplierId">Id del proveedor</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Funko actualizado</returns>
    public Task<bool> UpdatePriceFromFunkoAsync(long id, FunkoUpdatePriceRequestDto request, CancellationToken ct = default);

    // =========================================================
    // DELETE / RESTORE
    // =========================================================
    Task<bool> DeleteFunkoAsync(long id, CancellationToken ct);
    /// <summary>
    /// Obtener todos los funkos, incluyen su categoria
    /// no página
    /// </summary>
    /// <param name="ct">token de cancelación</param>
    /// <returns> Lista sin filtrar de funkos </returns>
    Task<Funko?> RestoreFunkoAsync(long id, CancellationToken ct);
}