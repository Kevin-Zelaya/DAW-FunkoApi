
using Microsoft.EntityFrameworkCore;

public class UserRepository(
    AppDbContext _context,
    ILogger<UserRepository> _logger
) : IUserRepository, IScoped
{
    // =========================================================
    // CREATE
    // =========================================================

    public async Task<User> CreateUserAsync(
        User user,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[REPOSITORY-CREATE] Creando usuario.");

        user.Id = 0;
        user.IsDeleted = false;
        user.CreatedAt = DateTime.UtcNow;
        user.LastLoginAt = null;
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;

        await _context.Users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "[REPOSITORY-CREATE] Usuario creado con ID {Id}.",
            user.Id);

        return user;
    }

    // =========================================================
    // READ
    // =========================================================

    public async Task<User?> GetUserByIdAsync(
        long id,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[REPOSITORY-GET] Buscando usuario con ID {Id}.",
            id);

        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Id == id && !u.IsDeleted,
                ct);
    }
    public async Task<User?> GetUserByEmailAsync(
        string email,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[REPOSITORY-GET] Buscando usuario por su email: {email}.",
            email);

        return await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Email == email && !u.IsDeleted,
                ct);
    }

    public async Task<List<User>> GetAllUsersAsync(
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[REPOSITORY-GETALL] Recuperando usuarios activos.");

        return await _context.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.Id)
            .ToListAsync(ct);
    }

    public async Task<(
        List<User> items,
        int count,
        int totalPages,
        bool HasPreviousPage,
        bool HasNextPage
    )> GetPaginatedUsersAsync(
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        if (pageNumber < 1)
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber));

        if (pageSize < 1)
            throw new ArgumentOutOfRangeException(
                nameof(pageSize));

        _logger.LogDebug(
            "[REPOSITORY-GETPAGINATED] Página {PageNumber}, tamaño {PageSize}.",
            pageNumber,
            pageSize);

        var query = _context.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted);

        var count = await query.CountAsync(ct);

        var totalPages = (int)Math.Ceiling(
            count / (double)pageSize);

        var items = await query
            .OrderBy(u => u.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (
            items,
            count,
            totalPages,
            pageNumber > 1,
            pageNumber < totalPages
        );
    }

    // =========================================================
    // UPDATE
    // =========================================================

    public async Task<User?> UpdateUserAsync(
        long id,
        User user,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[REPOSITORY-UPDATE] Actualizando usuario {Id}.",
            id);

        var existing = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Id == id && !u.IsDeleted,
                ct);

        if (existing is null)
            return null;

        // Solo datos generales modificables.
        existing.Username = user.Username;
        existing.Email = user.Email;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "[REPOSITORY-UPDATE] Usuario {Id} actualizado.",
            id);

        return existing;
    }

    public async Task<User?> UpdatePasswordAsync(
        long id,
        string passwordHash,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[REPOSITORY-UPDATEPASSWORD] Actualizando contraseña del usuario {Id}.",
            id);

        var existing = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Id == id && !u.IsDeleted,
                ct);

        if (existing is null)
            return null;

        existing.PasswordHash = passwordHash;

        // Invalidar sesiones renovables anteriores.
        existing.RefreshToken = null;
        existing.RefreshTokenExpiry = null;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "[REPOSITORY-UPDATEPASSWORD] Contraseña actualizada para el usuario {Id}.",
            id);

        return existing;
    }

    public async Task<User?> UpdateRoleAsync(
        long id,
        string role,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[REPOSITORY-UPDATEROLE] Actualizando rol del usuario {Id}.",
            id);

        var existing = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Id == id && !u.IsDeleted,
                ct);

        if (existing is null)
            return null;
        // Se intenta parsear el rol
        if(!Enum.TryParse<Role>(role, out Role rol))
            throw new BadHttpRequestException("Rol no encontrado");
        existing.Role = rol;
        
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "[REPOSITORY-UPDATEROLE] Rol actualizado para el usuario {Id}.",
            id);

        return existing;
    }

    // =========================================================
    // DELETE / RESTORE
    // =========================================================

    public async Task<bool> DeleteUserAsync(
        long id,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[REPOSITORY-DELETE] Eliminando usuario {Id}.",
            id);

        var existing = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Id == id && !u.IsDeleted,
                ct);

        if (existing is null)
            return false;

        existing.IsDeleted = true;

        // Invalidar el token de renovación.
        existing.RefreshToken = null;
        existing.RefreshTokenExpiry = null;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "[REPOSITORY-DELETE] Usuario {Id} eliminado lógicamente.",
            id);

        return true;
    }

    public async Task<User?> RestoreUserAsync(
        long id,
        CancellationToken ct = default)
    {
        _logger.LogDebug(
            "[REPOSITORY-RESTORE] Restaurando usuario {Id}.",
            id);

        var existing = await _context.Users
            .FirstOrDefaultAsync(
                u => u.Id == id && u.IsDeleted,
                ct);

        if (existing is null)
            return null;

        existing.IsDeleted = false;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation(
            "[REPOSITORY-RESTORE] Usuario {Id} restaurado.",
            id);

        return existing;
    }
}
