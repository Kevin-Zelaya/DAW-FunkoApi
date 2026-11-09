
using System.Drawing;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using MinimalApi.repositories;
using MinimalApi.Services.Email;

public class FunkoService(
    IFunkoRepository _repository,
    ISupplierService _supplierService,
    ICategoryService _categoryService,
    IStorageService _storageService,
    ILogger<FunkoService> _logger,
    ICache _cache,
    IFunkoPriceHistoryService _historyService,
    EmailQueueService _emailQueue,
    INotificationService _notificationService
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
        // Obtener categoria y proveedor
        var rs = _supplierService.GetSupplierByIdAsync(funko.SupplierId, ct);
        var rc = _categoryService.GetCategoryByIdAsync(funko.CategoryId, ct);
        await Task.WhenAll(rs, rc);
        var supplier = await rs;
        var category = await rc;
        // Comprobar el resultado de ambos
        if(supplier.IsFailure || category.IsFailure) // Si alguno de los dos es falla
            return supplier.IsFailure ? supplier.Error : category.Error; // se comprueba cual fue y se retorna el erro que ya es un result
        // Crear funko
        var response = await _repository.CreateFunkoAsync(
            funko.ToModel(), ct);
        // Agregamos al historia, este retornar un mensaje pero no es necesario para la respuesta
        await _historyService.AddHistoryAsync(
            response.Id,
            (decimal)response.Price, 
            0,
            ct
        );
        // Retornar como dto
        response.Category = category.Value!.ToEntity();
        return Result.Success<FunkoDto, DomainError>(response.ToDto(supplier.Value))
            .Tap(f => _logger.LogInformation("[SERVICE-CREATE] Funko creado correctamente con ID {Id}.", f.Id))
            .Tap(f => {
            _notificationService.NotifyResourceChangedAsync(
                new ResourceNotification<long>(
                    f.Id, "Funko", 
                    MethodsHttp.POST.ToString(), 
                    NotificationStatus.Ok.ToString(),
                    $"Funko creado correctamente con el id {f.Id}"));}) // notificar del cambio en el recurso
            .Tap(f => {
            _emailQueue.Enqueue(new EmailMessage{
                To = "admin@funkoapi.com",
                Subject = $"Nuevo Funko Creado: {response.Name}",
                Body = EmailTemplates.CreateBase(
                    "Funko Creado Exitosamente",
                    EmailTemplates.FunkoCreado(
                        response.Name,
                        (decimal)response.Price,
                        response.Id)),IsHtml = true});}); // Envío de correo
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
        var cached = await _cache.GetTAsync<FunkoDto>(key);
        if(cached is not null)
            return Result.Success<FunkoDto, DomainError>(cached) // Devuelve un Result<FunkoDto, DomainError>
                .Tap(funkoDto => _logger.LogInformation(
                    "[SERVICE-GET] Funko con ID {Id} recuperado correctamente desde la cache.", id));
        // buscar en la bd
        var response = await _repository.GetFunkoByIdAsync(id, ct);
        if(response is null)
            return Result.Failure<FunkoDto, DomainError>(
                    FunkoError.NotFound(id))
                    .Tap(_ => _logger.LogWarning(
                        "[SERVICE-GET] No se encontró el Funko con ID {Id}.",
                        id)); 
        // = Preparar respuesta ========
        // 1. Buscar proveedor
        // 2. mapear respuesta a funko dto pasandole el proveedor para mapear
        var toReturn = await _supplierService.GetSupplierByIdAsync(response.SupplierId)
            .Map(response.ToDto)
            .Tap(f => _logger.LogInformation("[SERVICE-GET] Funko con ID {Id} recuperado correctamente.", id));
        // Agregar a la caché
        try{await _cache.SetAsync(key, toReturn.Value);}
        catch(Exception ex){_logger.LogWarning(ex, "No se pudo guardar el funko {id} en la cache", id);}
        // Retornar
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

    /// <inheritdoc cref="IFunkoService.GetPaginatedFunkos(int, int, CancellationToken)"/>
    public async Task<Result<PaginatedResouce<FunkoDto>, DomainError>> GetPaginatedFunkos(
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        // Llave para paginación
        string key = $"FUNKO:ALL-PAGINATED:{pageSize}";
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
        var toReturn = response.items.Select(f => {
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
            await _cache.SetAsync(key, paginatedResponse, TimeSpan.FromSeconds(60));
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

        // Obtenemos el Funko actual antes de actualizar para conocer su precio previo
        var funkoActual = await _repository.GetFunkoByIdAsync(id, ct);
        if(funkoActual is null)
            return Result.Failure<FunkoDto, DomainError>(
                FunkoError.NotFound(id))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-UPDATE] No se encontró el Funko con ID {Id}.",
                    id));

        // Actualizar en la base de datos principal (Postgre/Mongo)
        var response = await _repository.UpdateFunkoAsync(id, request, ct);
        if(response is null)
            return Result.Failure<FunkoDto, DomainError>(
                FunkoError.NotFound(id))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-UPDATE] No se encontró el Funko con ID {Id}.",
                    id));
        var supplier = await _supplierService.GetSupplierByIdAsync(response.SupplierId); 
        
        // Invalidar en caché
        await _cache.RemoveAsync(key);
        // Retornar respuesta exitosa
        return Result.Success<FunkoDto, DomainError>(response.ToDto(supplier.Value))
                .Tap(f => _logger.LogInformation(
                    "[SERVICE-UPDATE] Funko con ID {Id} actualizado correctamente.",
                    f.Id))
                    .Tap(f => {_ = 
                        _notificationService.NotifyResourceChangedAsync(
                            new ResourceNotification<long>(
                                f.Id, "Funko", 
                                MethodsHttp.PUT.ToString(),
                                NotificationStatus.Ok.ToString(), 
                                $"Funko actualizado correctamente con el id {f.Id}"));});
    }
    /// <inheritdoc cref="IFunkoService.UpdatePriceAsync(long, FunkoUpdatePriceRequestDto, CancellationToken)"/>
    public async Task<Result<FunkoPriceUpdatedResponseDto, DomainError>> UpdatePriceAsync(
        long Id,
        FunkoUpdatePriceRequestDto request,
        CancellationToken ct = default)
    {
        string key = $"FUNKO:{Id}";
        // Obtenemos el Funko actual antes de actualizar para conocer su precio previo
        var funkoActual = await _repository.GetFunkoByIdAsync(Id, ct);
        if(funkoActual is null)
            return Result.Failure<FunkoPriceUpdatedResponseDto, DomainError>(
                FunkoError.NotFound(Id))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-UPDATE] No se encontró el Funko con ID {Id}.",
                    Id));
        // Comprobar si el precio ha cambiando
        double precioAnterior = funkoActual.Price;
        bool haCambiadoElPrecio = precioAnterior != request.Price;
        // =====================================

        // Actualizar en la base de dATOS
        var response = await _repository.UpdatePriceFromFunkoAsync(Id, request, ct);

        // Si el precio no cambio no hace falta llamar al historial de precios
        if (!haCambiadoElPrecio)
            return Result.Success<FunkoPriceUpdatedResponseDto, DomainError>(
                new FunkoPriceUpdatedResponseDto(
                    funkoActual.Price,
                    new PriceHistoryModifiedResponse("No ha sido necesario agregar el precio al historial ya que no ha cambiado.")
                )
            );
            
        var phResponse = await _historyService.AddHistoryAsync(
            Id, 
            (decimal)request.Price,
            (decimal)precioAnterior,
            ct
        );        
        // Invalidar en caché
        await _cache.RemoveAsync(key);
        // Retornar respuesta exitosa
        return Result.Success<FunkoPriceUpdatedResponseDto, DomainError>(
            new FunkoPriceUpdatedResponseDto(
                funkoActual.Price,
                phResponse.Value))
        .Tap(f => _logger.LogInformation(
            "[SERVICE-UPDATE] Funko con ID {Id} actualizado correctamente.",
            Id))
            .Tap(f => {_ = 
                    _notificationService.NotifyResourceChangedAsync(
                        new ResourceNotification<long>(
                            Id, 
                            "Funko", 
                            MethodsHttp.PATCH.ToString(), 
                            NotificationStatus.Ok.ToString(),
                            $"Precio del funko actualizado correctamente con el id {Id}"));});
    }
    /// <inheritdoc cref="IFunkoService.UpdateCategoryFromAsync(long, FunkoUpdateCategoryRequestDto, CancellationToken)"/>
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
    
        return Result.Success<FunkoDto, DomainError>(response.ToDto(supplier.Value))
                .Tap(f => _logger.LogInformation(
                    "[SERVICE-UPDATECATEGORY] Funko con ID {Id} actualizado correctamente.",
                    f.Id))
                .Tap(f => {_ = 
                    _notificationService.NotifyResourceChangedAsync(
                        new ResourceNotification<long>(
                            f.Id, "Funko", 
                            MethodsHttp.PATCH.ToString(), 
                            NotificationStatus.Ok.ToString(),
                            $"Categoria del funko con id {f.Id} actualizada correctamente"));});
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
        if(response is not null) // solo si lo encontró invalidar en caché
            await _cache.RemoveAsync(key);
        return response is not null
            ? Result.Success<FunkoDto, DomainError>(response.ToDto(supplier.Value))
                .Tap(f => _logger.LogInformation(
                    "[SERVICE-UPDATESUPPLIER] Funko con ID {Id} actualizado correctamente.",
                    f.Id))
                    .Tap(f => {_ = 
                        _notificationService.NotifyResourceChangedAsync(
                            new ResourceNotification<long>(
                                f.Id, "Funko", 
                                MethodsHttp.PATCH.ToString(), 
                                NotificationStatus.Ok.ToString(),
                                $"Proveedor del funko con id {f.Id} actualizado correctamente"));})
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
public async Task<Result<FunkoDeletedResponseDto, DomainError>> DeleteFunkoAsync(
    long id,
    CancellationToken ct = default)
{
    string key = $"FUNKO:{id}";

    _logger.LogDebug(
        "[SERVICE-DELETE] Intentando eliminar el Funko con ID {Id}.",
        id);

    // Comprobar y obtener el funko
    var exist = await _repository.GetFunkoByIdAsync(id, ct);

    if (exist is null)
        return Result.Failure<FunkoDeletedResponseDto, DomainError>(
            FunkoError.NotFound(id))
            .Tap(_ => _logger.LogWarning(
                "[SERVICE-DELETE] No se encontró el Funko con ID {Id}.",
                id));
    // Eliminar en la base de datos
    var response = await _repository.DeleteFunkoAsync(id, ct);
    // Eliminar en la caché
    await _cache.RemoveAsync(key);

    // Eliminar imagen en el storage service
    if (exist.Imagen is not null && exist.Imagen != "/uploads/funkos/default.png")
        await _storageService.DeleteFileAsync(exist.Imagen);

    // Eliminado lógico del historial
    var phResponse = await _historyService.DeleteHistoryAsync(id, ct);
    // Retornar resultado satisfactorio
    return Result.Success<FunkoDeletedResponseDto, DomainError>(
        new FunkoDeletedResponseDto(
            "Funko eliminado correctamente", // Se retorna este mensaje para acompañar al mensaje de eliminado del historial
            phResponse.Value
        ))
    .Tap(_ => _logger.LogInformation(
        "[SERVICE-DELETE] Funko con ID {Id} eliminado correctamente.",
        id))
    .Tap(f => {
            _emailQueue.Enqueue(new EmailMessage{
                To = "admin@funkoapi.com",
                Subject = $"Funko Eliminado: {exist.Name}",
                Body = EmailTemplates.CreateBase(
                    "Funko Eliminado del Sistema",
                    EmailTemplates.FunkoEliminado(exist.Name, exist.Id)
                ),
                IsHtml = true});}); // Envío de correo
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
        if(response is null)
            return Result.Failure<FunkoDto, DomainError>(
                FunkoError.NotFound(id))
                .Tap(_ => _logger.LogWarning(
                    "[SERVICE-Restore] No se encontró el Funko con ID {Id}.",
                    id));
        var supplier = await _supplierService.GetSupplierByIdAsync(response!.SupplierId, ct);
        Console.WriteLine(supplier.Value.Name+"--------------------------");
        return response is not null
            ? Result.Success<FunkoDto, DomainError>(response.ToDto(supplier.Value))
                .Tap(f => _logger.LogInformation(
                    "[SERVICE-RESTORE] Funko con ID {Id} restaurado correctamente.",
                    f.Id))
                    .Tap(f => {_ = 
                    _notificationService.NotifyResourceChangedAsync(
                        new ResourceNotification<long>(
                            id, "Funko", 
                            MethodsHttp.PATCH.ToString(), 
                            NotificationStatus.Ok.ToString(),
                            $"Funko restaurado correctamente con el id {id}"));})
            : Result.Failure<FunkoDto, DomainError>(
                FunkoError.NotFound(id))
                .Tap(_ => _logger.LogWarning("[SERVICE-RESTORE] No se encontró un Funko eliminado con ID {Id}.", id));
                
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
        // Obtener funko desde el repositorio por su id
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
                    "[SERVICE-UPDATE] No se pudo eliminar la imagen anterior del Funko con ID {Id}.", // TODO: proteger el resto de rutas
                    id));                                                                              // Crear usuario admin por defecto
        // Guardar nueva imagen en storage service                                                    // 
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
        // retornar según si es nulo o no
        return Result.Success<FunkoDto, DomainError>(exist.ToDto(supplier.Value)) // se le pasa el proveedor para convertir
                .Tap(f => _logger.LogInformation(
                    "[SERVICE-UPDATE] Funko con ID {Id} actualizado correctamente.",
                    f.Id))
                .Tap(f => {_ = 
                _notificationService.NotifyResourceChangedAsync(
                    new ResourceNotification<long>(
                        f.Id, "Funko", 
                        MethodsHttp.PATCH.ToString(), 
                        NotificationStatus.Ok.ToString(),
                        $"Imagen del funko con id {f.Id} actualizada correctamente"));});
    }
}
