using Microsoft.EntityFrameworkCore;
using Serilog;

public class CategoryPostgreRepository(
    AppDbContext _context
) : ICategoryRepository, IScoped
{
    // =========================================================
    // CREATE
    // =========================================================
    
    /// <inheritdoc cref="ICategoryRepository.CreateCategoryAsync(CategoryCreateRequestDto, CancellationToken)"/>
    public async Task<Category> CreateCategoryAsync(
        CategoryCreateRequestDto request, 
        CancellationToken ct)
    {
        var entidad = request.ToEntity();
        _context.Categories.Add(entidad);
        await _context.SaveChangesAsync(ct);
        return entidad;
    }

    // =========================================================
    // READ
    // =========================================================

    /// <inheritdoc cref="ICategoryRepository.GetAllCategoriesAsync(CancellationToken)"/>
    public async Task<List<Category>> GetAllCategoriesAsync(
        CancellationToken ct
    )
    {
        return await _context.Categories
                    .AsNoTracking()
                    .ToListAsync(ct);
    }
    /// <inheritdoc cref="ICategoryRepository.GetPaginatedCategoriesAsync(int, int, CancellationToken)"/>
    public async Task<(
            List<Category> items,
            int count,
            int totalPages,
            bool HasPreviousPage,
            bool HasNextPage
        )> GetPaginatedCategoriesAsync(
        int pageNumber,
        int pageSize,
        CancellationToken ct
    )
    {
        var response =  await _context.Categories
                    .AsNoTracking()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(ct);

        int count = response.Count();

        int totalPages = (int)Math.Ceiling(
            (double)count / pageSize);

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

    /// <inheritdoc cref="ICategoryRepository.GetCategoryByName(string, CancellationToken)"/>
    public async Task<Category?> GetCategoryByName(
        string name,
        CancellationToken ct
    )
    {
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name == name, ct);
    }
    /// <inheritdoc cref="ICategoryRepository.GetCategoryById(string, CancellationToken)"/>
    public async Task<Category?> GetCategoryById(
        string id,
        CancellationToken ct
    )
    {
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }
    /// <inheritdoc cref="ICategoryRepository.GetCategoryByName(string, CancellationToken)"/>
    public async Task<bool> ExistByName(
        string name,
        CancellationToken ct
    )
    {
        return await _context.Categories
            .AsNoTracking()
            .IgnoreQueryFilters()
            .AnyAsync(c => c.Name == name, ct);
    }

    // =========================================================
    // UPDATE
    // =========================================================
    
    /// <inheritdoc cref="ICategoryRepository.UpdateCategoryAsync(string, CategoryUpdateRequestDto, CancellationToken)"/>
    public async Task<Category?> UpdateCategoryAsync(
        string id,
        CategoryUpdateRequestDto request,
        CancellationToken ct
    )
    {
        var response = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if(response is null) return response;
        response.Name = request.Name;
        response.Description = request.Description;
        response.UpdatedAt = DateTime.UtcNow;
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

    // =========================================================
    // DELETE / RESTIRE
    // =========================================================
    
    /// <inheritdoc cref="ICategoryRepository.DeleteCategoryAsync(string, CancellationToken)"/>
    public async Task<bool> DeleteCategoryAsync(
        string id,
        CancellationToken ct)
    {
        var entidad = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
        entidad!.IsDelete = true;
        entidad.DeleteAT = DateTime.Now;
        await _context.SaveChangesAsync(ct);
        return true;
    }
    /// <inheritdoc cref="ICategoryRepository.RestoreCategoryAsync(string, CancellationToken)"/>
    public async Task<bool> RestoreCategoryAsync(
        string id,
        CancellationToken ct)
    {
        var entidad = await _context.Categories
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == id);
        entidad!.IsDelete = false;
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
