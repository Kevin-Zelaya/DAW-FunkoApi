
using CSharpFunctionalExtensions;

public class UserService(
    IUserRepository _repository,
    ILogger<UserService> _logger,
    ICache _cache
) : IScoped, IUserService
{
    // =========================================================
    // READ
    // =========================================================

    public async Task<Result<UserDto, DomainError>> GetUserByIdAsync(
        long id,
        CancellationToken ct = default)
    {
        string key = $"USER:{id}";

        _logger.LogDebug(
            "[SERVICE-GET] Buscando usuario {Id}.",
            id);

        // Buscar en caché
        var cached = await _cache.GetTAsync<UserDto>(key);

        if (cached is not null)
            return Result.Success<UserDto, DomainError>(cached)
                .Tap(_ => _logger.LogInformation(
                    "[SERVICE-GET] Usuario {Id} recuperado desde caché.",
                    id));

        // Buscar en repositorio
        var user = await _repository.GetUserByIdAsync(id, ct);

        if (user is null)
            return Result.Failure<UserDto, DomainError>(
                UserError.NotFound(id));

        var toReturn = user.ToDto();

        // Guardar en caché
        try
        {
            await _cache.SetAsync(key, toReturn);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "[SERVICE-GET] No se pudo guardar el usuario {Id} en caché.",
                id);
        }

        return Result.Success<UserDto, DomainError>(toReturn)
            .Tap(_ => _logger.LogInformation(
                "[SERVICE-GET] Usuario {Id} recuperado correctamente.",
                id));
    }

    public async Task<Result<UserDto, DomainError>> GetUserByEmailAsync(
        string email,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-GET] Buscando usuario por email {Email}.",
            email);

        var user = await _repository.GetUserByEmailAsync(email, ct);

        if (user is null)
            return Result.Failure<UserDto, DomainError>(
                UserError.NotFound(email));

        return Result.Success<UserDto, DomainError>(user.ToDto());
    }

    public async Task<Result<List<UserDto>, DomainError>> GetAllUsersAsync(
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[SERVICE-GETALL] Recuperando usuarios.");

        var users = await _repository.GetAllUsersAsync(ct);

        return Result.Success<List<UserDto>, DomainError>(
            users.Select(u => u.ToDto()).ToList())
            .Tap(response => _logger.LogInformation(
                "[SERVICE-GETALL] Se recuperaron {Count} usuarios.",
                response.Count));
    }

    /// <inheritdoc cref="IUserService.GetPaginatedUsersAsync(int, int, CancellationToken)"/>
    public async Task<Result<PaginatedResouce<UserDto>, DomainError>>
        GetPaginatedUsersAsync(
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
    {
        // Llave para paginación
        string key = $"USER:ALL-PAGINATED:{pageSize}";

        _logger.LogDebug(
            "[SERVICE-GETPAGINATEDUSERS] Recuperando página {PageNumber} con tamaño {PageSize}.",
            pageNumber, pageSize);

        // Si es la página 1, buscar en caché
        if (pageNumber == 1)
        {
            var cached =
                await _cache.GetTAsync<PaginatedResouce<UserDto>>(key);

            if (cached is not null)
                return Result.Success<PaginatedResouce<UserDto>, DomainError>(
                    cached)
                    .Tap(_ => _logger.LogInformation(
                        "[SERVICE-GETPAGINATEDUSERS] Se recuperaron {Count} usuarios de la página {PageNumber} desde la caché.",
                        cached.Count, pageNumber));
        }

        // Buscar en la base de datos
        var response = await _repository.GetPaginatedUsersAsync(
            pageNumber, pageSize, ct);

        // Transformar la respuesta al DTO de paginación
        var paginatedResponse = new PaginatedResouce<UserDto>(
            response.items.Select(u => u.ToDto()).ToList(),
            response.count,
            response.totalPages,
            response.HasPreviousPage,
            response.HasNextPage
        );

        // Guardar en caché solo si es la página 1 y contiene elementos
        if (response.count > 0 && pageNumber == 1)
            await _cache.SetAsync(
                key,
                paginatedResponse,
                TimeSpan.FromSeconds(60));

        return Result.Success<PaginatedResouce<UserDto>, DomainError>(
            paginatedResponse)
            .Tap(_ => _logger.LogInformation(
                "[SERVICE-GETPAGINATEDUSERS] Se recuperaron {Count} usuarios de la página {PageNumber}.",
                response.items.Count, pageNumber));
    }

    // =========================================================
    // UPDATE
    // =========================================================

    public async Task<Result<UserDto, DomainError>> UpdateUserAsync(
        long id,
        UserUpdateRegularRequestDto user,
        CancellationToken ct = default)
    {
        string key = $"USER:{id}";

        _logger.LogDebug(
            "[SERVICE-UPDATE] Actualizando usuario {Id}.",
            id);

        var updated = await _repository.UpdateUserAsync(id, user, ct);

        if (updated is null)
            return Result.Failure<UserDto, DomainError>(
                UserError.NotFound(id));

        // Invalidar caché
        await _cache.RemoveAsync(key);

        return Result.Success<UserDto, DomainError>(updated.ToDto())
            .Tap(_ => _logger.LogInformation(
                "[SERVICE-UPDATE] Usuario {Id} actualizado correctamente.",
                id));
    }

    public async Task<Result<UserDto, DomainError>> UpdatePasswordAsync(
        long id,
        string passwordHash,
        CancellationToken ct = default)
    {
        string key = $"USER:{id}";

        _logger.LogDebug(
            "[SERVICE-UPDATEPASSWORD] Actualizando contraseña del usuario {Id}.",
            id);

        var updated = await _repository.UpdatePasswordAsync(
            id,
            BCrypt.Net.BCrypt.HashPassword(
                passwordHash,
                workFactor: 11),
            ct);

        if (updated is null)
            return Result.Failure<UserDto, DomainError>(
                UserError.NotFound(id));

        // Invalidar caché
        await _cache.RemoveAsync(key);

        return Result.Success<UserDto, DomainError>(updated.ToDto())
            .Tap(_ => _logger.LogInformation(
                "[SERVICE-UPDATEPASSWORD] Contraseña actualizada para {Id}.",
                id));
    }

    public async Task<Result<UserDto, DomainError>> UpdateRoleAsync(
        long id,
        string role,
        CancellationToken ct = default)
    {
        string key = $"USER:{id}";

        _logger.LogDebug(
            "[SERVICE-UPDATEROLE] Actualizando rol del usuario {Id}.",
            id);

        var updated = await _repository.UpdateRoleAsync(id, role, ct);

        if (updated is null)
            return Result.Failure<UserDto, DomainError>(
                UserError.NotFound(id));

        // Invalidar caché
        await _cache.RemoveAsync(key);

        return Result.Success<UserDto, DomainError>(updated.ToDto())
            .Tap(_ => _logger.LogInformation(
                "[SERVICE-UPDATEROLE] Rol actualizado para {Id}.",
                id));
    }

    // =========================================================
    // DELETE / RESTORE
    // =========================================================

    public async Task<Result<bool, DomainError>> DeleteUserAsync(
        long id,
        CancellationToken ct = default)
    {
        string key = $"USER:{id}";

        _logger.LogDebug(
            "[SERVICE-DELETE] Eliminando usuario {Id}.",
            id);

        var deleted = await _repository.DeleteUserAsync(id, ct);

        if (!deleted)
            return Result.Failure<bool, DomainError>(
                UserError.NotFound(id));

        // Invalidar caché
        await _cache.RemoveAsync(key);

        return Result.Success<bool, DomainError>(true)
            .Tap(_ => _logger.LogInformation(
                "[SERVICE-DELETE] Usuario {Id} eliminado correctamente.",
                id));
    }

    public async Task<Result<UserDto, DomainError>> RestoreUserAsync(
        long id,
        CancellationToken ct = default)
    {
        string key = $"USER:{id}";

        _logger.LogDebug(
            "[SERVICE-RESTORE] Restaurando usuario {Id}.",
            id);

        var restored = await _repository.RestoreUserAsync(id, ct);

        if (restored is null)
            return Result.Failure<UserDto, DomainError>(
                UserError.NotFound(id));

        // Invalidar caché
        await _cache.RemoveAsync(key);

        return Result.Success<UserDto, DomainError>(restored.ToDto())
            .Tap(_ => _logger.LogInformation(
                "[SERVICE-RESTORE] Usuario {Id} restaurado correctamente.",
                id));
    }

    // =========================================================
    // CACHE MODELS
    // =========================================================

    private record UserPaginationCache(
        List<UserDto> Items,
        int Count,
        int TotalPages,
        bool HasPreviousPage,
        bool HasNextPage
    );
}
