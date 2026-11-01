using CSharpFunctionalExtensions;

public class SupplierService(
    ISupplierRepository _repository,
    ILogger<SupplierService> _logger,
    ICache _cache
) : ISupplierService, IScoped
{
    // =========================================================
    // CREATE
    // =========================================================

    /// <inheritdoc cref="ISupplierService.CreateSupplierAsync(SupplierCreateRequestDto, CancellationToken)"/>
    public async Task<Result<SupplierDto, DomainError>> CreateSupplierAsync(
        SupplierCreateRequestDto request,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-CREATESUPPLIER] Intentando crear un proveedor.");
        // Comprobar si existe proveedor con ese nombre
        var exist = await _repository.ExistByNameAsync(request.Name, ct);
        if(!exist)
            return Result.Failure<SupplierDto, DomainError>(
                ConflictError.Duplicate("Proveedor", request.Name));
        // Crear en repositorio
        var response = await _repository.CreateSupplierAsync(
            request, ct);

        return Result.Success<SupplierDto, DomainError>(response.ToDto())
            .Tap(s => _logger.LogInformation(
                "[SERVICE-CREATESUPPLIER] Proveedor creado correctamente con ID {Id}.",
                s.Id));
    }

    // =========================================================
    // READ
    // =========================================================

    /// <inheritdoc cref="ISupplierService.GetAllSuppliersAsync(CancellationToken)"/>
    public async Task<Result<List<SupplierDto>, DomainError>> GetAllSuppliersAsync(
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-GETSUPPLIERS] Recuperando todos los proveedores.");

        var response = await _repository.GetAllSuppliersAsync(ct);

        return Result.Success<List<SupplierDto>, DomainError>(
            response.Select(s => s.ToDto()).ToList())
            .Tap(s => _logger.LogInformation(
                "[SERVICE-GETSUPPLIERS] Se recuperaron {Count} proveedores.",
                s.Count));
    }

    /// <inheritdoc cref="ISupplierService.GetPaginatedSuppliersAsync(int, int, CancellationToken)"/>
    public async Task<Result<PaginatedResouce<SupplierDto>, DomainError>> GetPaginatedSuppliersAsync(
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        string key = $"SUPPLIER:ALL-PAGED:{pageNumber}";

        _logger.LogDebug(
            "[SERVICE-GETPAGINATEDSUPPLIERS] Recuperando página {PageNumber} con tamaño {PageSize}.",
            pageNumber, pageSize);

        if (pageNumber == 1)
        {
            var cached = await _cache.GetTAsync<PaginatedResouce<SupplierDto>>(key);

            if (cached is not null)
                return Result.Success<PaginatedResouce<SupplierDto>, DomainError>(
                    cached);
        }

        var response = await _repository.GetPaginatedSuppliersAsync(
            pageNumber, pageSize, ct);

        var paginatedResponse = new PaginatedResouce<SupplierDto>(
            response.items.Select(s => s.ToDto()).ToList(),
            response.count,
            response.totalPages,
            response.HasPreviousPage,
            response.HasNextPage);

        if (response.count > 0 && pageNumber == 1)
            await _cache.SetAsync(key, paginatedResponse);

        return Result.Success<PaginatedResouce<SupplierDto>, DomainError>(
            paginatedResponse)
            .Tap(s => _logger.LogInformation(
                "[SERVICE-GETPAGINATEDSUPPLIERS] Se recuperaron {Count} proveedores de la página {PageNumber} de {TotalPages}.",
                s.Items.Count, pageNumber, s.TotalPages));
    }

    /// <inheritdoc cref="ISupplierService.GetSupplierByIdAsync(string, CancellationToken)"/>
    public async Task<Result<SupplierDto, DomainError>> GetSupplierByIdAsync(
        string id,
        CancellationToken ct = default)
    {
        string key = $"SUPPLIER:{id}";

        _logger.LogDebug(
            "[SERVICE-GETSUPPLIER] Buscando proveedor con ID {Id}.", id);

        var cached = await _cache.GetTAsync<Supplier>(key);

        if (cached is not null)
            return Result.Success<SupplierDto, DomainError>(cached.ToDto())
                .Tap(_ => _logger.LogInformation(
                    "[SERVICE-GETSUPPLIER] Proveedor con ID {Id} recuperado desde la caché.",
                    id));

        var response = await _repository.GetSupplierByIdAsync(id, ct);

        if (response is not null)
            await _cache.SetAsync(key, response);

        return response is not null
            ? Result.Success<SupplierDto, DomainError>(response.ToDto())
                .Tap(s => _logger.LogInformation(
                    "[SERVICE-GETSUPPLIER] Proveedor con ID {Id} recuperado correctamente.",
                    s.Id))
            : Result.Failure<SupplierDto, DomainError>(
                NotFoundError.FromResource("Supplier", id))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-GETSUPPLIER] No se encontró el proveedor con ID {Id}.",
                    id));
    }

    // =========================================================
    // UPDATE
    // =========================================================

    /// <inheritdoc cref="ISupplierService.UpdateSupplierAsync(string, SupplierUpdateRequestDto, CancellationToken)"/>
    public async Task<Result<SupplierDto, DomainError>> UpdateSupplierAsync(
        string id,
        SupplierUpdateRequestDto request,
        CancellationToken ct = default)
    {
        string key = $"SUPPLIER:{id}";

        _logger.LogDebug(
            "[SERVICE-UPDATESUPPLIER] Intentando actualizar el proveedor con ID {Id}.",
            id);

        var response = await _repository.UpdateSupplierAsync(id, request, ct);

        if (response is not null)
        {
            await _cache.RemoveAsync(key);
            await _cache.RemoveAsync("SUPPLIER:ALL-PAGED:1");
        }

        return response is not null
            ? Result.Success<SupplierDto, DomainError>(response.ToDto())
                .Tap(s => _logger.LogInformation(
                    "[SERVICE-UPDATESUPPLIER] Proveedor con ID {Id} actualizado correctamente.",
                    s.Id))
            : Result.Failure<SupplierDto, DomainError>(
                NotFoundError.FromResource("Supplier", id))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-UPDATESUPPLIER] No se encontró el proveedor con ID {Id}.",
                    id));
    }

    // =========================================================
    // DELETE
    // =========================================================

    /// <inheritdoc cref="ISupplierService.DeleteSupplierAsync(string, CancellationToken)"/>
    public async Task<Result<bool, DomainError>> DeleteSupplierAsync(
        string id,
        CancellationToken ct = default)
    {
        string key = $"SUPPLIER:{id}";

        _logger.LogDebug(
            "[SERVICE-DELETESUPPLIER] Intentando eliminar el proveedor con ID {Id}.",
            id);

        var response = await _repository.DeleteSupplierAsync(id, ct);

        if (response)
        {
            await _cache.RemoveAsync(key);
            await _cache.RemoveAsync("SUPPLIER:ALL-PAGED:1");
        }

        return response
            ? Result.Success<bool, DomainError>(true)
                .Tap(_ => _logger.LogInformation(
                    "[SERVICE-DELETESUPPLIER] Proveedor con ID {Id} eliminado correctamente.",
                    id))
            : Result.Failure<bool, DomainError>(
                NotFoundError.FromResource("Supplier", id))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-DELETESUPPLIER] No se encontró el proveedor con ID {Id}.",
                    id));
    }

    // =========================================================
    // RESTORE
    // =========================================================

    /// <inheritdoc cref="ISupplierService.RestoreSupplierAsync(string, CancellationToken)"/>
    public async Task<Result<bool, DomainError>> RestoreSupplierAsync(
        string id,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-RESTORESUPPLIER] Intentando restaurar el proveedor con ID {Id}.",
            id);

        var response = await _repository.RestoreSupplierAsync(id, ct);

        return response
            ? Result.Success<bool, DomainError>(true)
                .Tap(_ => _logger.LogInformation(
                    "[SERVICE-RESTORESUPPLIER] Proveedor con ID {Id} restaurado correctamente.",
                    id))
            : Result.Failure<bool, DomainError>(
                NotFoundError.FromResource("Supplier", id))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-RESTORESUPPLIER] No se encontró el proveedor eliminado con ID {Id}.",
                    id));
    }
}