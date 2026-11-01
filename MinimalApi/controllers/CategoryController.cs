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
    public async Task<ActionResult> GetAllCategories(
        CancellationToken ct)
    {
        var response = await _service.GetAllCategoriesAsync(ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : BadRequest(response.Error);
    }

    /// <summary>
    /// Obtiene las categorías paginadas.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetPaginatedCategories(
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
            : BadRequest(response.Error);
    }

    /// <summary>
    /// Obtiene una categoría por su nombre.
    /// </summary>
    [HttpGet("{name}")]
    public async Task<ActionResult> GetCategoryByName(
        string name,
        CancellationToken ct)
    {
        var response = await _service.GetCategoryByNameAsync(name, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : NotFound(response.Error);
    }

    /// <summary>
    /// Obtiene una categoría por su ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetCategoryById(
        string id,
        CancellationToken ct)
    {
        var response = await _service.GetCategoryByIdAsync(id, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : NotFound(response.Error);
    }

    // =========================================================
    // CREATE
    // =========================================================

    /// <summary>
    /// Crea una nueva categoría.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateCategory(
        [FromBody] CategoryCreateRequestDto request,
        CancellationToken ct)
    {
        var response = await _service.CreateCategoryAsync(request, ct);

        return response.IsSuccess
            ? CreatedAtAction(
                nameof(GetCategoryByName),
                new { name = response.Value.Name },
                response.Value)
            : BadRequest(response.Error);
    }

    // =========================================================
    // UPDATE
    // =========================================================

    /// <summary>
    /// Actualiza una categoría existente.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCategory(
        string id,
        [FromBody] CategoryUpdateRequestDto request,
        CancellationToken ct)
    {
        var response = await _service.UpdateCategoryAsync(id, request, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : BadRequest(response.Error);
    }

    // =========================================================
    // DELETE
    // =========================================================

    /// <summary>
    /// Elimina lógicamente una categoría.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteCategory(
        string id,
        CancellationToken ct)
    {
        var response = await _service.DeleteCategoryAsync(id, ct);

        return response.IsSuccess
            ? NoContent()
            : NotFound(response.Error);
    }

    // =========================================================
    // RESTORE
    // =========================================================

    /// <summary>
    /// Restaura una categoría eliminada.
    /// </summary>
    [HttpPatch("{id:guid}/restore")]
    public async Task<ActionResult> RestoreCategory(
        string id,
        CancellationToken ct)
    {
        var response = await _service.RestoreCategoryAsync(id, ct);

        return response.IsSuccess
            ? NoContent()
            : NotFound(response.Error);
    }
}