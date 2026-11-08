using CSharpFunctionalExtensions;

public class CategoryService(
    ICategoryRepository _repository,
    ILogger<CategoryService> _logger,
    ICache _cache
) : ICategoryService, IScoped
{
    // =========================================================
    // CREATE
    // =========================================================

    /// <inheritdoc cref="ICategoryService.CreateCategoryAsync(CategoryCreateRequestDto, CancellationToken)"/>
    public async Task<Result<CategoryDto, DomainError>> CreateCategoryAsync(
        CategoryCreateRequestDto request,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-CREATECATEGORY] Intentando crear una categoría.");
        // Comporbar si existe categoría con ese nombre
        var exist = await _repository.ExistByName(request.Name, ct);
        if(exist)
            return Result.Failure<CategoryDto, DomainError>(
                ConflictError.Duplicate("Categoria", request.Name));
        // Crear categoría
        var response = await _repository.CreateCategoryAsync(request, ct);

        return Result.Success<CategoryDto, DomainError>(response.ToDto())
            .Tap(c => _logger.LogInformation(
                "[SERVICE-CREATECATEGORY] Categoría creada correctamente con ID {Id}.",
                c.Id));
    }

    // =========================================================
    // READ
    // =========================================================

    /// <inheritdoc cref="ICategoryService.GetAllCategoriesAsync(CancellationToken)"/>
    public async Task<Result<List<CategoryDto>, DomainError>> GetAllCategoriesAsync(
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-GETALLCATEGORIES] Recuperando todas las categorías.");

        var response = await _repository.GetAllCategoriesAsync(ct);

        return Result.Success<List<CategoryDto>, DomainError>(
            response.Select(c => c.ToDto()).ToList())
            .Tap(_ => _logger.LogInformation(
                "[SERVICE-GETALLCATEGORIES] Se recuperaron {Count} categorías.",
                response.Count));
    }

    /// <inheritdoc cref="ICategoryService.GetPaginatedCategoriesAsync(int, int, CancellationToken)"/>
    public async Task<Result<PaginatedResouce<CategoryDto>, DomainError>>
        GetPaginatedCategoriesAsync(
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
    {
        // Llave para paginación
        string key = $"CATEGORY:ALL-PAGINATED:{pageNumber}";

        _logger.LogDebug(
            "[SERVICE-GETPAGINATEDCATEGORIES] Recuperando página {PageNumber} con tamaño {PageSize}.",
            pageNumber, pageSize);

        // Si es la página 1, buscar en caché
        if (pageNumber == 1)
        {
            var cached =
                await _cache.GetTAsync<PaginatedResouce<CategoryDto>>(key);

            if (cached is not null)
                return Result.Success<PaginatedResouce<CategoryDto>, DomainError>(
                    cached)
                    .Tap(_ => _logger.LogInformation(
                        "[SERVICE-GETPAGINATEDCATEGORIES] Se recuperaron {Count} categorías de la página {PageNumber} desde la caché.",
                        cached.Count, pageNumber));
        }

        // Buscar en la base de datos
        var response = await _repository.GetPaginatedCategoriesAsync(
            pageNumber, pageSize, ct);

        // Transformar la respuesta al DTO de paginación
        var paginatedResponse = new PaginatedResouce<CategoryDto>(
            response.items.Select(c => c.ToDto()).ToList(),
            response.count,
            response.totalPages,
            response.HasPreviousPage,
            response.HasNextPage
        );

        // Guardar en caché solo si es la página 1 y contiene elementos
        if (response.count > 0 && pageNumber == 1)
            await _cache.SetAsync(key, paginatedResponse);

        return Result.Success<PaginatedResouce<CategoryDto>, DomainError>(
            paginatedResponse)
            .Tap(_ => _logger.LogInformation(
                "[SERVICE-GETPAGINATEDCATEGORIES] Se recuperaron {Count} categorías de la página {PageNumber}.",
                response.items.Count, pageNumber));
    }

    /// <inheritdoc cref="ICategoryService.GetCategoryByNameAsync(string, CancellationToken)"/>
    public async Task<Result<CategoryDto?, DomainError>> GetCategoryByNameAsync(
        string name,
        CancellationToken ct)
    {
        _logger.LogDebug(
            "[SERVICE-GETCATEGORYBYNAME] Buscando categoría con nombre {Name}.",
            name);

        var response = await _repository.GetCategoryByName(name, ct);

        return response is not null
            ? Result.Success<CategoryDto?, DomainError>(response.ToDto())
                .Tap(c => _logger.LogInformation(
                    "[SERVICE-GETCATEGORYBYNAME] Categoría con ID {Id} recuperada correctamente.",
                    c!.Id))
            : Result.Failure<CategoryDto?, DomainError>(
                NotFoundError.FromResource("Category", name))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-GETCATEGORYBYNAME] No se encontró la categoría {Name}.",
                    name));
    }
    
    /// <inheritdoc cref="ICategoryService.GetCategoryByIdAsync(string, CancellationToken)"/>
    public async Task<Result<CategoryDto?, DomainError>> GetCategoryByIdAsync(
        string id,
        CancellationToken ct)
    {
        _logger.LogDebug(
            "[SERVICE-GETCATEGORYBYID] Buscando categoría con ID {Id}.",
            id);

        var response = await _repository.GetCategoryById(id, ct);

        return response is not null
            ? Result.Success<CategoryDto?, DomainError>(response.ToDto())
                .Tap(c => _logger.LogInformation(
                    "[SERVICE-GETCATEGORYBYID] Categoría con ID {Id} recuperada correctamente.",
                    c!.Id))
            : Result.Failure<CategoryDto?, DomainError>(
                NotFoundError.FromResource("Category", id))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-GETCATEGORYBYID] No se encontró la categoría con ID {Id}.",
                    id));
    }

    // =========================================================
    // UPDATE
    // =========================================================

    /// <inheritdoc cref="ICategoryService.UpdateCategoryAsync(Guid, CategoryUpdateRequestDto, CancellationToken)"/>
    public async Task<Result<CategoryDto, DomainError>> UpdateCategoryAsync(
        string id,
        CategoryUpdateRequestDto request,
        CancellationToken ct = default)
    {
        string key = $"CATEGORY:{id}";

        _logger.LogDebug(
            "[SERVICE-UPDATECATEGORY] Intentando actualizar la categoría con ID {Id}.",
            id);

        // Actualizar en la base de datos
        var response = await _repository.UpdateCategoryAsync(id, request, ct);

        if (response is not null)
        {
            // Invalidar en caché
            await _cache.RemoveAsync(key);
            await _cache.RemoveAsync("CATEGORY:ALL-PAGINATED:1");
        }

        return response is not null
            ? Result.Success<CategoryDto, DomainError>(response.ToDto())
                .Tap(c => _logger.LogInformation(
                    "[SERVICE-UPDATECATEGORY] Categoría con ID {Id} actualizada correctamente.",
                    c.Id))
            : Result.Failure<CategoryDto, DomainError>(
                NotFoundError.FromResource("Category", id.ToString()))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-UPDATECATEGORY] No se encontró la categoría con ID {Id}.",
                    id));
    }

    // =========================================================
    // DELETE
    // =========================================================

    /// <inheritdoc cref="ICategoryService.DeleteCategoryAsync(Guid, CancellationToken)"/>
    public async Task<Result<bool, DomainError>> DeleteCategoryAsync(
        string id,
        CancellationToken ct = default)
    {
        string key = $"CATEGORY:{id}";

        _logger.LogDebug(
            "[SERVICE-DELETECATEGORY] Intentando eliminar la categoría con ID {Id}.",
            id);

        // Eliminar lógicamente en la base de datos
        var response = await _repository.DeleteCategoryAsync(id, ct);

        if (response)
        {
            // Invalidar en caché
            await _cache.RemoveAsync(key);
            await _cache.RemoveAsync("CATEGORY:ALL-PAGINATED:1");
        }

        return response
            ? Result.Success<bool, DomainError>(true)
                .Tap(_ => _logger.LogInformation(
                    "[SERVICE-DELETECATEGORY] Categoría con ID {Id} eliminada correctamente.",
                    id))
            : Result.Failure<bool, DomainError>(
                NotFoundError.FromResource("Category", id.ToString()))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-DELETECATEGORY] No se encontró la categoría con ID {Id}.",
                    id));
    }

    // =========================================================
    // RESTORE
    // =========================================================

    /// <inheritdoc cref="ICategoryService.RestoreCategoryAsync(Guid, CancellationToken)"/>
    public async Task<Result<bool, DomainError>> RestoreCategoryAsync(
        string id,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-RESTORECATEGORY] Intentando restaurar la categoría con ID {Id}.",
            id);

        var response = await _repository.RestoreCategoryAsync(id, ct);

        return response
            ? Result.Success<bool, DomainError>(true)
                .Tap(_ => _logger.LogInformation(
                    "[SERVICE-RESTORECATEGORY] Categoría con ID {Id} restaurada correctamente.",
                    id))
            : Result.Failure<bool, DomainError>(
                NotFoundError.FromResource("Category", id.ToString()))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-RESTORECATEGORY] No se encontró la categoría eliminada con ID {Id}.",
                    id));
    }
}
