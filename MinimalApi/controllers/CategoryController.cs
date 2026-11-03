using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/categories")]
public class CategoryController(
    ICategoryService _service
) : ControllerBase
{
    // =========================================================
    // READ
    // =========================================================

    /// <summary>
    /// Obtiene todas las categorías.
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<List<CategoryDto>>> GetAllCategories(
        CancellationToken ct)
    {
        var result = await _service.GetAllCategoriesAsync(ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.ToHttpResult<List<CategoryDto>>();
    }

    /// <summary>
    /// Obtiene las categorías paginadas.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResouce<CategoryDto>>> GetPaginatedCategories(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest(
                "La página y el tamaño deben ser mayores que 0.");

        var response = await _service.GetPaginatedCategoriesAsync(
            pageNumber, pageSize, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<PaginatedResouce<CategoryDto>>();
    }

    /// <summary>
    /// Obtiene una categoría por su ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetCategoryById(
        string id,
        CancellationToken ct)
    {
        var response = await _service.GetCategoryByIdAsync(id, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<CategoryDto>();
    }

    // =========================================================
    // CREATE
    // =========================================================

    /// <summary>
    /// Crea una nueva categoría.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> CreateCategory(
        [FromBody] CategoryCreateRequestDto request,
        CancellationToken ct)
    {
        var response = await _service.CreateCategoryAsync(request, ct);

        return response.IsSuccess
            ? CreatedAtAction(
                nameof(GetCategoryById),
                new { id = response.Value.Id },
                response.Value)
            : response.Error.ToHttpResult<CategoryDto>();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    /// <summary>
    /// Actualiza una categoría existente.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(
        string id,
        [FromBody] CategoryUpdateRequestDto request,
        CancellationToken ct)
    {
        var response = await _service.UpdateCategoryAsync(id, request, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<CategoryDto>();
    }

    // =========================================================
    // DELETE
    // =========================================================

    /// <summary>
    /// Elimina lógicamente una categoría.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> DeleteCategory(
        string id,
        CancellationToken ct)
    {
        var response = await _service.DeleteCategoryAsync(id, ct);

        return response.IsSuccess
            ? NoContent()
            : response.Error.ToHttpResult<CategoryDto>();
    }

    // =========================================================
    // RESTORE
    // =========================================================

    /// <summary>
    /// Restaura una categoría eliminada.
    /// </summary>
    [HttpPatch("{id:guid}/restore")]
    public async Task<ActionResult<CategoryDto>> RestoreCategory(
        string id,
        CancellationToken ct)
    {
        var response = await _service.RestoreCategoryAsync(id, ct);

        return response.IsSuccess
            ? NoContent()
            : response.Error.ToHttpResult<CategoryDto>();
    }
}