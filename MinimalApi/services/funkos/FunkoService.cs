
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class FunkoService(
    IFunkoRepository _repository,
    ISupplierService _supplierService,
    IStorageService _storageService,
    ILogger<FunkoService> _logger,
    ICache _cache
) : IFunkoService, IScoped
{
    // =========================================================
    // CREATE
    // =========================================================

    /// <inheritdoc cref="IFunkoService.CreateFunkoAsync(FunkoCreateRequestDto, CancellationToken)"/>
    public async Task<Result<FunkoDto, DomainError>> CreateFunkoAsync(
        FunkoCreateRequestDto funko,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-CREATE] Intentando crear un Funko.");
        // Comprobar si existe algún con ese username
        var exist = await _repository.ExistByNameAsync(funko.Name, ct);
        if(exist)
            return Result.Failure<FunkoDto, DomainError>(
                ConflictError.Duplicate("Funko", funko.Name));
        // Comprobar si existe el proveedor y obtenerlo
        var supplier = await _supplierService.GetSupplierByIdAsync(funko.SupplierId, ct);
        if(supplier.Value is null)
            return Result.Failure<FunkoDto, DomainError>(
                NotFoundError.FromResource("Supplier", funko.SupplierId));
        // Crear funko
        var response = await _repository.CreateFunkoAsync(
            funko.ToModel(), ct);
        // Retornar como dto
        return Result.Success<FunkoDto, DomainError>(response.ToDto(supplier.Value))
            .Tap(f => _logger.LogInformation(
                "[SERVICE-CREATE] Funko creado correctamente con ID {Id}.",
                f.Id));
    }

    // =========================================================
    // READ
    // =========================================================

    /// <inheritdoc cref="IFunkoService.GetFunkoByIdAsync(long, CancellationToken)"/>
    public async Task<Result<FunkoDto, DomainError>> GetFunkoByIdAsync( 
        long id,
        CancellationToken ct)
    {
        // Llave para buscar en caché
        string key = $"FUNKO:{id}";
        _logger.LogDebug(
            "[SERVICE-GET] Buscando Funko con ID {Id}.", id);

        // 1. Buscar en caché
        var cached = await _cache.GetTAsync<Funko>(key);
        if(cached is not null) {

            var result = await _supplierService.GetSupplierByIdAsync(cached.SupplierId)
                .Map(cached.ToDto)
                .Tap(funkoDto => _logger.LogInformation(
                    "[SERVICE-GET] Funko con ID {Id} recuperado correctamente desde la cache.", id));

            return result; // Devuelve un Result<FunkoDto, DomainError>
        } 
        // buscar en la bd
        var response = await _repository.GetFunkoByIdAsync(id, ct);
        if(response is null)
            return Result.Failure<FunkoDto, DomainError>(
                    FunkoError.NotFound(id))
                    .Tap(_ => _logger.LogWarning(
                        "[SERVICE-GET] No se encontró el Funko con ID {Id}.",
                        id)); 
        // Agregar a la caché
        try{await _cache.SetAsync(key, response);
        // Buscar proveedor en el servicio
        await _cache.RemoveAsync("FUNKO:ALL-PAGINATED:1"); }// Eliminar páginación cacheada
        catch(Exception ex){_logger.LogWarning(ex, "No se pudo guardar el funko {id} en la cache", id);}
        // = Preparar respuesta ========
        // 1. Buscar proveedor
        // 2. mapear respuesta a funko dto pasandole el proveedor para mapear
        var toReturn = await _supplierService.GetSupplierByIdAsync(response.SupplierId)
            .Map(response.ToDto)
            .Tap(f => _logger.LogInformation(
                    "[SERVICE-GET] Funko con ID {Id} recuperado correctamente.", id));
        
        return toReturn;
                
    }

    /// <inheritdoc cref="IFunkoService.GetAllFunkos(CancellationToken)"/>
    public async Task<Result<List<FunkoDto>, DomainError>> GetAllFunkos(
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-GETALL] Recuperando todos los Funkos.");
        // Obtener de forma asíncrona funkos y provedores
        var response = _repository.GetAllFunkosAsync(ct);
        var responseSuppliers = _supplierService.GetAllSuppliersAsync(ct); // quería cachear pero mucho problema por invalidaciones y concurrencia
        await Task.WhenAll(response, responseSuppliers); 
        // Transformar la respuesta de los proveedores en un diccionario para mejorar busquedas
        var dictionary = responseSuppliers.Result.Value.ToDictionary(s => s.Id); // a diccionario
        // Por cada funko asignar su proveedor y convertir a dto
        var toReturn = response.Result.AsParallel().Select(f => {
            if(dictionary.TryGetValue(f.SupplierId, out var supplier))
            {
                return f.ToDto(supplier);
            }
            return f.ToDto(new Supplier());
        }).ToList();
        // Retornar nueva lista con proveedores asignados
        return Result.Success<List<FunkoDto>, DomainError>(
            toReturn)
            .Tap(_ => _logger.LogInformation(
                "[SERVICE-GETALL] Se recuperaron {Count} Funkos.",
                toReturn.Count));
    }

    /// <inheritdoc cref="IFunkoService.GetPaginatedFunkos(string, string, CancellationToken)"/>
    public async Task<Result<PaginatedResouce<FunkoDto>, DomainError>> GetPaginatedFunkos(
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        // Llave para paginación
        string key = $"FUNKO:ALL-PAGINATED:{pageNumber}";
        _logger.LogDebug(
            "[SERVICE-GETPAGINATED] Recuperando página {PageNumber} con tamaño {PageSize}.",
            pageNumber, pageSize);
        if(pageNumber == 1) // Si es la página 1, busca en caché
        {
            var cached = await _cache.GetTAsync<PaginatedResouce<FunkoDto>>(key);
            if(cached is not null) // si lo encuentra retorna directamente
                return Result.Success<PaginatedResouce<FunkoDto>, DomainError>(
                    cached)
                .Tap(_ => _logger.LogInformation(
                    "[SERVICE-GETPAGINATED] Se recuperaron {Count} Funkos de la página {PageNumber} desde el caché.",
                        cached.Count, pageNumber));
        }
        // Buscar en la base de datos
        var frepository = _repository.GetPaginatedFunkosAsync(
            pageNumber, pageSize, ct);
        var suppliers = _supplierService.GetAllSuppliersAsync(ct);
        await Task.WhenAll(frepository, suppliers);
        // Transformar la respuesta de los proveedores en un diccionario para mejorar busquedas
        var dictionary = suppliers.Result.Value.ToDictionary(s => s.Id); // a diccionario
        var response = await frepository; // obtener valor
        // Por cada funko asignar su proveedor y convertir a dto
        var toReturn = response.items.AsParallel().Select(f => {
            if(dictionary.TryGetValue(f.SupplierId, out var supplier))
            {
                return f.ToDto(supplier);
            }
            return f.ToDto(new Supplier());
        }).ToList();
        // Transpofmar la respuesta en un dto que representa mejor la paginación
        var paginatedResponse = new PaginatedResouce<FunkoDto>(
                toReturn,
                response.count,
                response.totalPages,
                response.HasPreviousPage,
                response.HasNextPage
            );
        // Guardar en caché solo si es la página 1 y contiene elementos
        if(response.count > 0 && pageNumber == 1)
            await _cache.SetAsync(key, paginatedResponse);
        return Result.Success<PaginatedResouce<FunkoDto>, DomainError>(
                paginatedResponse)
            .Tap(_ => _logger.LogInformation(
                "[SERVICE-GETPAGINATED] Se recuperaron {Count} Funkos de la página {PageNumber}.",
                response.items.Count, pageNumber));
    }

    // =========================================================
    // UPDATE
    // =========================================================

    /// <inheritdoc cref="IFunkoService.UpdateFunkoAsync(long, FunkoUpdateRequestDto, CancellationToken)"/>
    public async Task<Result<FunkoDto, DomainError>> UpdateFunkoAsync(
        long id,
        FunkoUpdateRequestDto request,
        CancellationToken ct = default)
    {
        string key = $"FUNKO:{id}";
        _logger.LogDebug(
            "[SERVICE-UPDATE] Intentando actualizar el Funko con ID {Id}.",
            id);
        // Actualizar en db
        var response = await _repository.UpdateFunkoAsync(id, request, ct);
        if(response is null)
            return Result.Failure<FunkoDto, DomainError>(
                FunkoError.NotFound(id))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-UPDATE] No se encontró el Funko con ID {Id}.",
                    id));

        var supplier = await _supplierService.GetSupplierByIdAsync(response.SupplierId); // ejecutar si el anterior funciona
        // Invalidar en caché
        await _cache.RemoveAsync(key);
        await _cache.RemoveAsync("FUNKO:ALL-PAGINATED:1"); // Eliminar páginación cacheada
        // retornar según si es nulo o no
        return Result.Success<FunkoDto, DomainError>(response.ToDto(supplier.Value)) // se le pasa el proveedor para convertir
                .Tap(f => _logger.LogInformation(
                    "[SERVICE-UPDATE] Funko con ID {Id} actualizado correctamente.",
                    f.Id));
    }
    /// <inheritdoc cref="IFunkoService.UpdateCategoryFromAsync(long, CategoryUpdateRequestDto, CancellationToken)"/>
    public async Task<Result<FunkoDto, DomainError>> UpdateCategoryFromAsync(
        long id,
        FunkoUpdateCategoryRequestDto request,
        CancellationToken ct = default)
    {
        string key = $"FUNKO:{id}";
        _logger.LogDebug(
            "[SERVICE-UPDATECATEGORY] Intentando actualizar la categoría del Funko con el ID {Id}.",
            id);
        // Actualizar la categoría en la db
        var response = await _repository.UpdateCategoryFromFunkoAsync(id, request, ct);
        
        if(response is null)
            return Result.Failure<FunkoDto, DomainError>(
                FunkoError.NotFound(id))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-UPDATECATEGORY] No se encontró el Funko con ID {Id}.",
                    id));
        
            var supplier = await _supplierService.GetSupplierByIdAsync(response.SupplierId); // ejecutar si el anterior funciona
            // Invalidar en caché
            await _cache.RemoveAsync(key);
            await _cache.RemoveAsync("FUNKO:ALL-PAGINATED:1"); // Eliminar páginación cacheada
        
        return Result.Success<FunkoDto, DomainError>(response.ToDto(supplier.Value))
                .Tap(f => _logger.LogInformation(
                    "[SERVICE-UPDATECATEGORY] Funko con ID {Id} actualizado correctamente.",
                    f.Id));
    }
    /// <inheritdoc cref="IFunkoService.UpdateSupplierFromFunko(long, FunkoUpdateSupplierDto, CancellationToken)"/>
    public async Task<Result<FunkoDto, DomainError>> UpdateSupplierFromFunko(
        long id,
        FunkoUpdateSupplierDto request,
        CancellationToken ct = default)
    {
        string key = $"FUNKO:{id}";
        _logger.LogDebug(
            "[SERVICE-UPDATECATEGORY] Intentando actualizar la categoría del Funko con el ID {Id}.",
            id);
        var supplier = await _supplierService.GetSupplierByIdAsync(request.SupplierId, ct);
        // Verificar si existe el proveedor 
        if(supplier.IsFailure)
            return Result.Failure<FunkoDto, DomainError>(
                NotFoundError.FromResource("Supplier", request.SupplierId));
        // Actualizar en la base de datos
        var response = await _repository.UpdateSuplierFromFunkoAsync(id, request, ct);
        if(response is not null) // solo si lo encontró
        {// Invalidar en caché
            await _cache.RemoveAsync(key);
            await _cache.RemoveAsync("FUNKO:ALL-PAGINATED:1"); // Eliminar páginación cacheada
        }
        return response is not null
            ? Result.Success<FunkoDto, DomainError>(response.ToDto(supplier.Value))
                .Tap(f => _logger.LogInformation(
                    "[SERVICE-UPDATESUPPLIER] Funko con ID {Id} actualizado correctamente.",
                    f.Id))
            : Result.Failure<FunkoDto, DomainError>(
                FunkoError.NotFound(id))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-UPDATESUPPLIER] No se encontró el Funko con ID {Id}.",
                    id));
    }



    // =========================================================
    // DELETE / RESTORE
    // =========================================================

    /// <inheritdoc cref="IFunkoService.DeleteFunkoAsync(long, CancellationToken)"/>
    public async Task<Result<bool, DomainError>> DeleteFunkoAsync(
        long id,
        CancellationToken ct = default)
    {
        string key = $"FUNKO:{id}";
        _logger.LogDebug(
            "[SERVICE-DELETE] Intentando eliminar el Funko con ID {Id}.",
            id);
        // Comprobar y obtener el funko
        var exist = await _repository.GetFunkoByIdAsync(id, ct);
        if(exist is null)
            return Result.Failure<bool, DomainError>(
                FunkoError.NotFound(id))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-DELETE] No se encontró el Funko con ID {Id}.",
                    id));
        // Eliminar en la base de datos
        var response = await _repository.DeleteFunkoAsync(id, ct);
        // Eliminar en la caché
        if(response) {
            await _cache.RemoveAsync(key);
            await _cache.RemoveAsync("FUNKO:ALL-PAGINATED:1"); // Eliminar páginación cacheada
        }
        // Eliminar imaegen en el storage service
        if(exist.Imagen is not null && exist.Imagen != "/uploads/funkos/default.png")
            await _storageService.DeleteFileAsync(exist.Imagen);
        
        // Retornar según si es verdadero o false la eliminación
        return Result.Success<bool, DomainError>(true)
                .Tap(_ => _logger.LogInformation(
                    "[SERVICE-DELETE] Funko con ID {Id} eliminado correctamente.",
                    id));
    }

    /// <inheritdoc cref="IFunkoService.RestoreDeletedFunkoAsync(long, CancellationToken)"/>
    public async Task<Result<FunkoDto, DomainError>> RestoreDeletedFunkoAsync(
        long id,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-RESTORE] Intentando restaurar el Funko con ID {Id}.",
            id);

        var response = await _repository.RestoreFunkoAsync(id, ct);
        var supplier = await _supplierService.GetSupplierByIdAsync(response!.SupplierId, ct);
        return response is not null
            ? Result.Success<FunkoDto, DomainError>(response.ToDto(supplier.Value))
                .Tap(f => _logger.LogInformation(
                    "[SERVICE-RESTORE] Funko con ID {Id} restaurado correctamente.",
                    f.Id))
            : Result.Failure<FunkoDto, DomainError>(
                FunkoError.NotFound(id))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-RESTORE] No se encontró un Funko eliminado con ID {Id}.",
                    id));
    }

    // =========================================================
    // UPDATE IMAGE
    // =========================================================

    /// <inheritdoc cref="IFunkoService.UpdateFunkoAsync(long, FunkoUpdateRequestDto, CancellationToken)"/>
    public async Task<Result<FunkoDto, DomainError>> UpdateImageFromFunkoAsync(
        long id,
        IFormFile file,
        CancellationToken ct = default)
    {
        string key = $"FUNKO:{id}";
        _logger.LogDebug(
            "[SERVICE-UPDATE] Intentando actualizar la imagen del Funko con ID {Id}.",
            id);
        // Obtener funko
        var exist = await _repository.GetFunkoByIdAsync(id, ct);
        if(exist is null)
            return Result.Failure<FunkoDto, DomainError>(
                FunkoError.NotFound(id))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-UPDATE] No se encontró el Funko con ID {Id}.",
                    id));

        // Eliminar anterior en storage service
        var delete = await _storageService.DeleteFileAsync(exist.Imagen!);
        if(delete.IsFailure)
            return Result.Failure<FunkoDto, DomainError>(
                StorageError.ErrorEliminando())
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-UPDATE] No se pudo eliminar la imagen anterior del Funko con ID {Id}.",
                    id));
        // Guardar nueva imagen en storage service
        var save = await _storageService.SaveFileAsync(file, "funkos");
        if(save.IsFailure)
            return Result.Failure<FunkoDto, DomainError>(
                StorageError.ErrorGuardando())
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-UPDATE] No se pudo guardar la nueva imagen del Funko con ID {Id}.",
                    id));
        // Actualizar en la base de datos
        var response = await _repository.UpdateImageFromFunkoAsync(id, save.Value, ct);
        if(!response)
            return Result.Failure<FunkoDto, DomainError>(
                StorageError.ErrorEliminando())
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-UPDATE] No se pudo agregar la nueva imagen del Funko con ID {Id}.",
                    id));
        exist.Imagen = save.Value; // actualizar la propiedad de la imagen en el objeto existente
        var supplier = await _supplierService.GetSupplierByIdAsync(exist.SupplierId); // ejecutar si el anterior 
        // Invalidar en caché
        await _cache.RemoveAsync(key);
        await _cache.RemoveAsync("FUNKO:ALL-PAGINATED:1"); // Eliminar páginación cacheada
        // retornar según si es nulo o no
        return Result.Success<FunkoDto, DomainError>(exist.ToDto(supplier.Value)) // se le pasa el proveedor para convertir
                .Tap(f => _logger.LogInformation(
                    "[SERVICE-UPDATE] Funko con ID {Id} actualizado correctamente.",
                    f.Id));
    }
}
