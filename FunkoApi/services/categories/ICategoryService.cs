
using CSharpFunctionalExtensions;

/// <summary>
/// Define las operaciones de servicio para la gestión de categorías.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Obtiene todas las categorías.
    /// </summary>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Lista de categorías o un error de dominio.</returns>
    Task<Result<List<CategoryDto>, DomainError>> GetAllCategoriesAsync(CancellationToken ct);

    /// <summary>
    /// Obtiene una categoría por su nombre.
    /// </summary>
    /// <param name="name">Nombre de la categoría.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Categoría encontrada o error de dominio.</returns>
    Task<Result<CategoryDto?, DomainError>> GetCategoryByNameAsync(string name, CancellationToken ct);

    /// <summary>
    /// Obtiene una categoría por su ID.
    /// </summary>
    /// <param name="id">ID de la categoría.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Categoría encontrada o error de dominio.</returns>
    Task<Result<CategoryDto, DomainError>> GetCategoryByIdAsync(string id, CancellationToken ct);

    /// <summary>
    /// Obtiene una lista paginada de categorías.
    /// </summary>
    /// <param name="pageNumber">Número de página solicitada.</param>
    /// <param name="pageSize">Cantidad de categorías por página.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Categorías paginadas con sus metadatos o un error de dominio.</returns>
    Task<Result<PaginatedResouce<CategoryDto>, DomainError>> GetPaginatedCategoriesAsync(int pageNumber, int pageSize, CancellationToken ct);

    /// <summary>
    /// Crea una nueva categoría.
    /// </summary>
    /// <param name="request">Datos necesarios para crear la categoría.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Categoría creada o un error de dominio.</returns>
    Task<Result<CategoryDto, DomainError>> CreateCategoryAsync(CategoryCreateRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Actualiza los datos de una categoría existente.
    /// </summary>
    /// <param name="id">Identificador de la categoría que se desea actualizar.</param>
    /// <param name="request">Nuevos datos de la categoría.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>Categoría actualizada o un error de dominio.</returns>
    Task<Result<CategoryDto, DomainError>> UpdateCategoryAsync(string id, CategoryUpdateRequestDto request, CancellationToken ct = default);

    /// <summary>
    /// Elimina lógicamente una categoría.
    /// </summary>
    /// <param name="id">Identificador de la categoría que se desea eliminar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>true si la categoría se eliminó correctamente o un error de dominio.</returns>
    Task<Result<bool, DomainError>> DeleteCategoryAsync(string id, CancellationToken ct = default);

    /// <summary>
    /// Restaura una categoría eliminada lógicamente.
    /// </summary>
    /// <param name="id">Identificador de la categoría que se desea restaurar.</param>
    /// <param name="ct">Token de cancelación.</param>
    /// <returns>true si la categoría se restauró correctamente o un error de dominio.</returns>
    Task<Result<bool, DomainError>> RestoreCategoryAsync(string id, CancellationToken ct = default);
}