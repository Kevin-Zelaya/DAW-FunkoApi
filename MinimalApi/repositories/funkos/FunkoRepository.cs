
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Serilog;

/// <summary>
/// Repositorio de acceso a datos de Funkos mediante PostgreSQL.
/// </summary>
/// 
/// TODO: Hacer control de concurrencia en los patch y updates
public class FunkoRepository(
    AppDbContext _context
) : IFunkoRepository, IScoped
{
    // =========================================================
    // CREATE
    // =========================================================

    /// <inheritdoc cref="IFunkoRepository.CreateFunkoAsync(Funko, CancellationToken)"/>
    public async Task<Funko> CreateFunkoAsync(
        Funko funko,
        CancellationToken ct = default)
    {

        _context.Funkos.Add(funko);

        await _context.SaveChangesAsync(ct);

        await _context.Entry(funko) // Obtener el proveedor al que hace referencia
            .Reference(f => f.Category)
            .LoadAsync(ct);

        return funko;
    }

    // =========================================================
    // READ
    // =========================================================

    /// <inheritdoc cref="IFunkoRepository.GetFunkoByIdAsync(long, CancellationToken)"/>
    public async Task<Funko?> GetFunkoByIdAsync(
    long id,
        CancellationToken ct = default)
    {
        Funko? funko = await _context.Funkos
            .AsNoTracking()
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == id, ct);
        return funko;
    } // 

    /// <inheritdoc cref="IFunkoRepository.GetAllFunkosAsync(CancellationToken)"/>
    public async Task<List<Funko>> GetAllFunkosAsync(CancellationToken ct = default)
    {
        // 1. Cargar colecciones founkos/Proveedores
        List<Funko> funkos = await _context.Funkos
            .AsNoTracking()
            .Include(f => f.Category)
            .ToListAsync(ct);

        return funkos;
    }

    /// <inheritdoc cref="IFunkoRepository.GetPaginatedFunkosAsync(int, int, CancellationToken)"/>
    public async Task<(
        List<Funko> items,
        int count,
        int totalPages,
        bool HasPreviousPage,
        bool HasNextPage
    )> GetPaginatedFunkosAsync(
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        // 1. Cargar colecciones founkos/Proveedores
        List<Funko> funkos= await _context.Funkos
            .AsNoTracking()
            .OrderBy(f => f.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        // 3. Calcular metadatos
        int count = funkos.Count;
        int totalPages = (int)Math.Ceiling(
            (double)count / pageSize);
        bool hasPreviousPage = pageNumber > 1;
        bool hasNextPage = pageNumber < totalPages;

        return (
            funkos,
            count,
            totalPages,
            hasPreviousPage,
            hasNextPage
        );
    }
    /// <inheritdoc cref="IFunkoRepository.ExistByNameAsync(string, CancellationToken)"/>
    public async Task<bool> ExistByNameAsync(
    string name,
    CancellationToken ct = default)
    {
        return await _context.Funkos
            .AsNoTracking()
            .IgnoreQueryFilters()
            .AnyAsync(f => f.Name == name, ct);
    } // 

    // =========================================================
    // UPDATE
    // =========================================================

    /// <inheritdoc cref="IFunkoRepository.UpdateFunkoAsync(long, FunkoUpdateRequestDto, CancellationToken)"/>
    public async Task<Funko?> UpdateFunkoAsync(
        long id,
        FunkoUpdateRequestDto funko,
        CancellationToken ct = default)
    {
        var response = await _context.Funkos
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        if (response is null)
            return null;

        response.Name = funko.Name;
        response.Franchise = funko.Franchise;
        response.Number = funko.Number;
        response.Exclusive = funko.Exclusive;
        response.Price = funko.Price;
        response.Imagen = funko.Imagen ?? "default.png";
        response.UpdatedAt = DateTime.UtcNow;

        try
        {
            _context.Entry(response)
                .Property(f => f.RowVersion)
                .OriginalValue = funko.RowVersion;
            await _context.SaveChangesAsync(ct);
        } catch(DbUpdateConcurrencyException)
        {
            Log.Error("[REPOSITORY-UPDATE] Error de concurrencia, al actualizar el funko con id {id}", id);
            throw;
        }

        return response;
    }
    /// <inheritdoc cref="IFunkoRepository.UpdateCategoryFromFunkoAsync(long, CategoryUpdateRequestDto, CancellationToken)"/>
    public async Task<Funko?> UpdateCategoryFromFunkoAsync(
        long id,
        FunkoUpdateCategoryRequestDto request,
        CancellationToken ct = default
    )
    {
        var response = await _context.Funkos
            .FirstOrDefaultAsync(f => f.Id == id, ct);
        response?.CategoryId = request.CategoryId;

        if (response is null)
            return null;
        
        try
        {
            _context.Entry(response)
                .Property(f => f.RowVersion)
                .OriginalValue = request.RowVersion;
            await _context.SaveChangesAsync(ct);
        } catch(DbUpdateConcurrencyException)
        {
            Log.Error("[REPOSITORY-UPDATE] Error de concurrencia, al actualizar el funko con id {id}", id);
            throw;
        }

        return response;
    }
    /// <inheritdoc cref="IFunkoRepository.UpdateSuplierFromFunkoAsync(long, FunkoUpdateSupplierDto, CancellationToken)"/>
    public async Task<Funko?> UpdateSuplierFromFunkoAsync(
        long id,
        FunkoUpdateSupplierDto request,
        CancellationToken ct = default
    )
    {
        var response = await _context.Funkos
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        if (response is null)
            return null;
        response.SupplierId = request.SupplierId;
        try
        {
            _context.Entry(response)
                .Property(f => f.RowVersion)
                .OriginalValue = request.RowVersion;
            await _context.SaveChangesAsync(ct);
        } catch(DbUpdateConcurrencyException)
        {
            Log.Error("[REPOSITORY-UPDATE] Error de concurrencia, al actualizar el funko con id {id}", id);
            throw;
        }
        return response;
    }
    /// <inheritdoc cref="IFunkoRepository.UpdateImageFromFunkoAsync(long, string, CancellationToken)"/>
    public async Task<bool> UpdateImageFromFunkoAsync(
        long id,
        string image,
        CancellationToken ct = default
    )
    {
        var response = await _context.Funkos
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        if (response is null)
            return false;
        response.Imagen = image;
        await _context.SaveChangesAsync(ct);

        return true;
    }

    
    // =========================================================
    // DELETE / RESTORE
    // =========================================================

    /// <inheritdoc cref="IFunkoRepository.DeleteFunkoAsync(long, CancellationToken)"/>
    public async Task<bool> DeleteFunkoAsync(
        long id,
        CancellationToken ct = default)
    {
        var response = await _context.Funkos
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        if (response is null)
            return false;

        response.IsDelete = true;
        response.DeleteAT = DateTime.UtcNow;
        response.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return true;
    }

    /// <inheritdoc cref="IFunkoRepository.RestoreFunkoAsync(long, CancellationToken)"/>
    public async Task<Funko?> RestoreFunkoAsync(
        long id,
        CancellationToken ct = default)
    {
        var response = await _context.Funkos
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(f => f.Id == id && f.IsDelete, ct);

        if (response is null)
            return null;

        response.IsDelete = false;
        response.DeleteAT = null;
        response.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        return response;
    }

}
