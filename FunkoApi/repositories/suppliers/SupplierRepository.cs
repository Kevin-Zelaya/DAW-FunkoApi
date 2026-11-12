using Microsoft.EntityFrameworkCore;
using Serilog;

public class SuppliersRepository(
    AppDbMongoContext _context
) : ISupplierRepository, IScoped
{

    // =========================================================
    // CREATE
    // =========================================================

    /// <inheritdoc cref="ISupplierRepository.CreateSupplierAsync(SupplierCreateRequestDto, CancellationToken)"/>
    public async Task<Supplier> CreateSupplierAsync(
        SupplierCreateRequestDto request,
        CancellationToken ct)
    {
        var entity = request.ToEntity();
        _context.Suppliers.Add(entity);
        await _context.SaveChangesAsync(ct);

        return entity;
    }

    // =========================================================
    // READ
    // =========================================================

    /// <inheritdoc cref="ISupplierRepository.GetAllSuppliersAsync(CancellationToken)"/>
    public async Task<List<Supplier>> GetAllSuppliersAsync(
        CancellationToken ct)
    {
        return await _context.Suppliers
            .AsNoTracking()
            .ToListAsync(ct);
    }

    /// <inheritdoc cref="ISupplierRepository.GetPaginatedSuppliersAsync(int, int, CancellationToken)"/>
    public async Task<(
        List<Supplier> items,
        int count,
        int totalPages,
        bool HasPreviousPage,
        bool HasNextPage
    )> GetPaginatedSuppliersAsync(
        int pageNumber,
        int pageSize,
        CancellationToken ct)
    {
        var query = _context.Suppliers
            .AsNoTracking()
            .OrderBy(s => s.Id);

        int count = await query.CountAsync(ct);

        int totalPages = (int)Math.Ceiling(
            (double)count / pageSize);

        var response = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        bool hasPreviousPage = pageNumber > 1;
        bool hasNextPage = pageNumber < totalPages;

        return (
            response,
            count,
            totalPages,
            hasPreviousPage,
            hasNextPage
        );
    }

    /// <inheritdoc cref="ISupplierRepository.GetSupplierByIdAsync(string, CancellationToken)"/>
    public async Task<Supplier?> GetSupplierByIdAsync(
        string id,
        CancellationToken ct)
    {
        return await _context.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }
    /// <inheritdoc cref="ISupplierRepository.ExistByNameAsync(string, CancellationToken)"/>
    public async Task<bool> ExistByNameAsync(
    string name,
    CancellationToken ct = default)
    {
        return await _context.Suppliers
            .AsNoTracking()
            .IgnoreQueryFilters()
            .AnyAsync(f => f.Name == name, ct);
    } 
    
    // =========================================================
    // UPDATE
    // =========================================================

    /// <inheritdoc cref="ISupplierRepository.UpdateSupplierAsync(string, SupplierUpdateRequestDto, CancellationToken)"/>
    public async Task<Supplier?> UpdateSupplierAsync(
        string id,
        SupplierUpdateRequestDto request,
        CancellationToken ct)
    {
        var response = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (response is null)
            return null;

        response.Name = request.Name;

        try{
            _context.Entry(response)
                .Property(s => s.RowVersion)
                .OriginalValue = request.RowVersion;
            await _context.SaveChangesAsync(ct);
        } catch (DbUpdateConcurrencyException ex)
        {
            Log.Error(
                ex,
                "[REPOSITORY-UPDATE] Error de concurrencia al actualizar el proveedor con ID {Id}.",
                id);

            throw;
        }

        return response;
    }

    // =========================================================
    // DELETE / RESTORE
    // =========================================================
    
    /// <inheritdoc cref="ISupplierRepository.DeleteSupplierAsync(string, CancellationToken)"/>
    public async Task<bool> DeleteSupplierAsync(
        string id,
        CancellationToken ct)
    {
        var entidad = await _context.Suppliers
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (entidad is null)
            return false;

        entidad.IsDeleted = true;
        entidad.DeleteAt = DateTime.Now;

        await _context.SaveChangesAsync(ct);

        return true;
    }

    /// <inheritdoc cref="ISupplierRepository.RestoreSupplierAsync(string, CancellationToken)"/>
    public async Task<bool> RestoreSupplierAsync(
        string id,
        CancellationToken ct)
    {
        var entidad = await _context.Suppliers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (entidad is null)
            return false;

        entidad.IsDeleted = false;
        entidad.DeleteAt = null;

        await _context.SaveChangesAsync(ct);

        return true;
    }
}