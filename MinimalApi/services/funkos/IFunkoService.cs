using CSharpFunctionalExtensions;

public interface IFunkoService
{
    /// <summary>
    /// Crear funko de forma asincorna
    /// mediante dto de petición
    /// </summary>
    /// <param name="funko">Dto con reglas para crear</param>
    /// <see cref="FunkoCreateRequestDto"/>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Entidad ya creada con id asignado</returns>
    Task<Result<FunkoDto, DomainError>> CreateFunkoAsync(FunkoCreateRequestDto funko, CancellationToken ct = default);
    /// <summary>
    /// Obtener funko por su id
    /// </summary>
    /// <param name="id">Id del funko a obtener</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Funko solicitado</returns>
    Task<Result<FunkoDto, DomainError>> GetFunkoByIdAsync(long id, CancellationToken ct);
    /// <summary>
    /// Actualizar funko mediante
    /// dto de actualización
    /// </summary>
    /// <param name="id">Id del usuarios a actualizar</param>
    /// <param name="request">Dto con reglas pra actualizar
    /// no contiene categoria para permitir cambio a partir de 
    /// actualización parcial con "path"/// </param>
    /// <see cref="FunkoUpdateRequestDto"/>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Funko actualizado</returns>
    public Task<Result<FunkoDto, DomainError>> UpdateFunkoAsync(long id, FunkoUpdateRequestDto request, CancellationToken ct = default);
    /// <summary>
    /// Actualizar categoría del
    /// funko
    /// </summary>
    /// <param name="id">Id del usuarios a actualizar</param>
    /// <param name="request">Dto de categoría con reglas para actualizar</param>
    /// <see cref="CategoryUpdateRequestDto"/>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Funko actualizado</returns>
    public Task<Result<FunkoDto, DomainError>> UpdateCategoryFromAsync(long id, FunkoUpdateCategoryRequestDto request, CancellationToken ct = default);
    /// <summary>
    /// Actualizar proveedor de
    /// funko
    /// </summary>
    /// <param name="id">Id del usuarios a actualizar</param>
    /// <param name="SupplierId">Id del proveedor</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Funko actualizado</returns>
    public Task<Result<FunkoDto, DomainError>> UpdateSupplierFromFunko(long id, FunkoUpdateSupplierDto SupplierId, CancellationToken ct = default);
    /// <summary>
    /// Obtener lista completa de funkos
    /// se utiliza para pruebas, en producción
    /// no es lo mejor
    /// </summary>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Lista de funkos sin páginar</returns>
    
    public Task<Result<List<FunkoDto>, DomainError>> GetAllFunkos(CancellationToken ct = default);
    /// <summary>
    /// Obtener lista páginada de funkos
    /// </summary>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Lista páginada de funkos</returns>
    public Task<Result<PaginatedResouce<FunkoDto>, DomainError>> GetPaginatedFunkos(int pageNumber, int pageSize, CancellationToken ct = default);
    /// <summary>
    /// Eliminar funko por su iid
    /// </summary>
    /// <param name="id">id del funko a eliminar</param>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>True o false dependiendo del resultado</returns>
    public Task<Result<bool, DomainError>> DeleteFunkoAsync(long id, CancellationToken ct);
    /// <summary>
    /// Restaurar funko eliminado a partir de su id
    /// </summary>
    /// <param name="ct">Token de cancelación</param>
    /// <returns>Funko restaurada</returns>
    public Task<Result<FunkoDto, DomainError>> RestoreDeletedFunkoAsync(long id, CancellationToken ct);

}