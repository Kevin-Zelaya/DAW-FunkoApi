using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
[ApiController]
[Route("api/categories")]
public class CategoryController(
    ICategoryService _service
) : ControllerBase
{
    // =========================================================
    // READ
    // =========================================================

    [HttpGet("all")]
    [SwaggerOperation(
        Summary = "Obtener todas las categorías",
        Description = "Devuelve el listado completo de categorías."
    )]
    [SwaggerResponse(200, "Categorías obtenidas correctamente.", typeof(List<CategoryDto>))]
    public async Task<ActionResult<List<CategoryDto>>> GetAllCategories(
        CancellationToken ct)
    {
        var result = await _service.GetAllCategoriesAsync(ct);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.ToHttpResult<List<CategoryDto>>();
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Obtener categorías paginadas",
        Description = "Devuelve las categorías divididas en páginas según los parámetros indicados."
    )]
    [SwaggerResponse(200, "Página obtenida correctamente.", typeof(PaginatedResouce<CategoryDto>))]
    [SwaggerResponse(400, "El número de página o el tamaño son inválidos.")]
    public async Task<ActionResult<PaginatedResouce<CategoryDto>>> GetPaginatedCategories(
        [FromQuery, SwaggerParameter("Número de página (mínimo 1).")]
        int pageNumber = 1,

        [FromQuery, SwaggerParameter("Cantidad de categorías por página (mínimo 1).")]
        int pageSize = 10,

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

    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Obtener una categoría por ID",
        Description = "Busca y devuelve una categoría mediante su identificador."
    )]
    [SwaggerResponse(200, "Categoría encontrada.", typeof(CategoryDto))]
    [SwaggerResponse(404, "No se ha encontrado la categoría.")]
    public async Task<ActionResult<CategoryDto>> GetCategoryById(
        [SwaggerParameter("Identificador de la categoría.")]
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

    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    [SwaggerOperation(
        Summary = "Crear una categoría",
        Description = "Crea una nueva categoría. Solo accesible para administradores."
    )]
    [SwaggerResponse(201, "Categoría creada correctamente.", typeof(CategoryDto))]
    [SwaggerResponse(400, "Los datos proporcionados no son válidos.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(409, "Conflicto al crear la categoría.")]
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

    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Actualizar una categoría",
        Description = "Actualiza los datos de una categoría existente. Solo accesible para administradores."
    )]
    [SwaggerResponse(200, "Categoría actualizada correctamente.", typeof(CategoryDto))]
    [SwaggerResponse(400, "Los datos proporcionados no son válidos.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(404, "Categoría no encontrada.")]
    public async Task<ActionResult<CategoryDto>> UpdateCategory(
        [SwaggerParameter("Identificador de la categoría.")]
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

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Eliminar una categoría",
        Description = "Realiza un borrado lógico de una categoría. Solo accesible para administradores."
    )]
    [SwaggerResponse(204, "Categoría eliminada correctamente.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(404, "Categoría no encontrada.")]
    public async Task<ActionResult<CategoryDto>> DeleteCategory(
        [SwaggerParameter("Identificador GUID de la categoría.")]
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

    [Authorize(Roles = "ADMIN")]
    [HttpPatch("{id:guid}/restore")]
    [SwaggerOperation(
        Summary = "Restaurar una categoría",
        Description = "Restaura una categoría previamente eliminada. Solo accesible para administradores."
    )]
    [SwaggerResponse(204, "Categoría restaurada correctamente.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(404, "Categoría no encontrada.")]
    public async Task<ActionResult<CategoryDto>> RestoreCategory(
        [SwaggerParameter("Identificador GUID de la categoría.")]
        string id,
        CancellationToken ct)
    {
        var response = await _service.RestoreCategoryAsync(id, ct);

        return response.IsSuccess
            ? NoContent()
            : response.Error.ToHttpResult<CategoryDto>();
    }
}