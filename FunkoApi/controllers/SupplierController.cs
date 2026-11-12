
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("api/suppliers")]
public class SupplierController(
    ISupplierService _service
) : ControllerBase
{
    // =========================================================
    // CREATE
    // =========================================================

    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    [SwaggerOperation(
        Summary = "Crear un proveedor",
        Description = "Crea un nuevo proveedor. Solo accesible para administradores."
    )]
    [SwaggerResponse(201, "Proveedor creado correctamente.")]
    [SwaggerResponse(400, "Los datos proporcionados no son válidos.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(409, "Conflicto al crear el proveedor.")]
    public async Task<ActionResult> CreateSupplier(
        [FromBody] SupplierCreateRequestDto request,
        CancellationToken ct)
    {
        var response = await _service.CreateSupplierAsync(request, ct);

        return response.IsSuccess
                ? CreatedAtAction(
                nameof(GetSupplierById),
                new { controller = "Supplier", id = response.Value.Id },
                response.Value)
            : BadRequest(response.Error);
    }

    // =========================================================
    // READ
    // =========================================================

    [Authorize(Roles = "ADMIN")]
    [HttpGet("all")]
    [SwaggerOperation(
        Summary = "Obtener todos los proveedores",
        Description = "Devuelve el listado completo de proveedores. Solo accesible para administradores."
    )]
    [SwaggerResponse(200, "Proveedores obtenidos correctamente.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(400, "Error al obtener los proveedores.")]
    public async Task<ActionResult> GetAllSuppliers(
        CancellationToken ct)
    {
        var response = await _service.GetAllSuppliersAsync(ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : BadRequest(response.Error);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet]
    [SwaggerOperation(
        Summary = "Obtener proveedores paginados",
        Description = "Devuelve una página de proveedores según el número de página y el tamaño solicitado. Solo accesible para administradores."
    )]
    [SwaggerResponse(200, "Página obtenida correctamente.")]
    [SwaggerResponse(400, "El número de página o el tamaño son inválidos.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    public async Task<ActionResult> GetPaginatedSuppliers(
        [FromQuery, SwaggerParameter("Número de página (mínimo 1).")]
        int pageNumber = 1,

        [FromQuery, SwaggerParameter("Cantidad de proveedores por página (mínimo 1).")]
        int pageSize = 10,

        CancellationToken ct = default)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest(
                "La página y el tamaño deben ser mayores que 0.");

        var response = await _service.GetPaginatedSuppliersAsync(
            pageNumber, pageSize, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : BadRequest(response.Error);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Obtener un proveedor por ID",
        Description = "Busca y devuelve un proveedor mediante su identificador. Solo accesible para administradores."
    )]
    [SwaggerResponse(200, "Proveedor encontrado.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(404, "Proveedor no encontrado.")]
    public async Task<ActionResult> GetSupplierById(
        [SwaggerParameter("Identificador del proveedor.")]
        string id,
        CancellationToken ct)
    {
        var response = await _service.GetSupplierByIdAsync(id, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : NotFound(response.Error);
    }

    // =========================================================
    // UPDATE
    // =========================================================

    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Actualizar un proveedor",
        Description = "Actualiza los datos de un proveedor existente. Solo accesible para administradores."
    )]
    [SwaggerResponse(200, "Proveedor actualizado correctamente.")]
    [SwaggerResponse(400, "Los datos proporcionados no son válidos.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(404, "Proveedor no encontrado.")]
    public async Task<ActionResult> UpdateSupplier(
        [SwaggerParameter("Identificador del proveedor.")]
        string id,
        [FromBody] SupplierUpdateRequestDto request,
        CancellationToken ct)
    {
        var response = await _service.UpdateSupplierAsync(id, request, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : NotFound(response.Error);
    }

    // =========================================================
    // DELETE
    // =========================================================

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Eliminar un proveedor",
        Description = "Realiza un borrado lógico de un proveedor. Solo accesible para administradores."
    )]
    [SwaggerResponse(204, "Proveedor eliminado correctamente.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(404, "Proveedor no encontrado.")]
    public async Task<ActionResult> DeleteSupplier(
        [SwaggerParameter("Identificador del proveedor.")]
        string id,
        CancellationToken ct)
    {
        var response = await _service.DeleteSupplierAsync(id, ct);

        return response.IsSuccess
            ? NoContent()
            : NotFound(response.Error);
    }

    // =========================================================
    // RESTORE
    // =========================================================

    [Authorize(Roles = "ADMIN")]
    [HttpPatch("{id}/restore")]
    [SwaggerOperation(
        Summary = "Restaurar un proveedor",
        Description = "Restaura un proveedor eliminado lógicamente. Solo accesible para administradores."
    )]
    [SwaggerResponse(204, "Proveedor restaurado correctamente.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(404, "Proveedor no encontrado.")]
    public async Task<ActionResult> RestoreSupplier(
        [SwaggerParameter("Identificador del proveedor.")]
        string id,
        CancellationToken ct)
    {
        var response = await _service.RestoreSupplierAsync(id, ct);

        return response.IsSuccess
            ? NoContent()
            : NotFound(response.Error);
    }
}