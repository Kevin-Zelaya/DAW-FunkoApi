using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.config;
using MinimalApi.repositories;
using Serilog;
using Swashbuckle.AspNetCore.Annotations;

[ApiController]
[Route("api/funkos")]
public class FunkoController(
    IFunkoService _service,
    IFunkoPriceHistoryService _historyService
) : ControllerBase
{
    // =========================================================
    // CREATE
    // =========================================================

    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    [SwaggerOperation(
        Summary = "Crear un Funko",
        Description = "Crea un nuevo Funko. Solo accesible para administradores."
    )]
    [SwaggerResponse(201, "Funko creado correctamente.", typeof(FunkoDto))]
    [SwaggerResponse(400, "Los datos proporcionados no son válidos.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(409, "Conflicto al crear el Funko.")]
    public async Task<ActionResult<FunkoDto>> CreateFunko(
        [FromBody] FunkoCreateRequestDto dto) // para obtener claim email
    {
        var email = User.FindFirst("email")?.Value;

        if (string.IsNullOrWhiteSpace(email))
            return Unauthorized("El token no contiene el email del usuario.");
        var response = await _service.CreateFunkoAsync(dto, email);

        return response.IsSuccess
            ? CreatedAtAction(nameof(GetFunkoById),
                new { id = response.Value.Id }, response.Value)
            : response.Error.ToHttpResult<FunkoDto>();
    }

    // =========================================================
    // READ
    // =========================================================

    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Obtener un Funko por ID",
        Description = "Devuelve los datos de un Funko identificado por su ID."
    )]
    [SwaggerResponse(200, "Funko encontrado.", typeof(FunkoDto))]
    [SwaggerResponse(404, "No se ha encontrado el Funko.")]
    public async Task<ActionResult<FunkoDto>> GetFunkoById(
        [SwaggerParameter("Identificador del Funko.")]
        long id,
        CancellationToken ct)
    {
        var funko = await _service.GetFunkoByIdAsync(id, ct);

        return funko.IsSuccess
            ? Ok(funko.Value)
            : funko.Error.ToHttpResult<FunkoDto>();
    }

    [HttpGet("all")]
    [SwaggerOperation(
        Summary = "Obtener todos los Funkos",
        Description = "Devuelve el listado completo de Funkos."
    )]
    [SwaggerResponse(200, "Listado de Funkos obtenido correctamente.", typeof(List<FunkoDto>))]
    public async Task<ActionResult<List<FunkoDto>>> GetAllFunkos(
        CancellationToken ct)
    {
        var funkos = await _service.GetAllFunkos(ct);

        return funkos.IsSuccess
            ? Ok(funkos.Value)
            : funkos.Error.ToHttpResult<List<FunkoDto>>();
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Obtener Funkos paginados",
        Description = "Devuelve una página de Funkos según el número de página y el tamaño solicitado."
    )]
    [SwaggerResponse(200, "Página obtenida correctamente.", typeof(PaginatedResouce<FunkoDto>))]
    [SwaggerResponse(400, "El número de página o el tamaño son inválidos.")]
    public async Task<ActionResult<PaginatedResouce<FunkoDto>>> GetPaginatedFunkos(
        [FromQuery, SwaggerParameter("Número de página (mínimo 1).")]
        int pageNumber = 1,

        [FromQuery, SwaggerParameter("Cantidad de elementos por página (mínimo 1).")]
        int pageSize = 5,

        CancellationToken ct = default)
    {
        Log.Information(
            "PageNumber: {PageNumber}, PageSize: {PageSize}",
            pageNumber, pageSize);

        if (pageNumber < 1 || pageSize < 1)
            return BadRequest("La página y el tamaño deben ser mayores que 0.");

        var funkos = await _service.GetPaginatedFunkos(
            pageNumber, pageSize, ct);

        return funkos.IsSuccess
            ? Ok(funkos.Value)
            : funkos.Error.ToHttpResult<PaginatedResouce<FunkoDto>>();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Actualizar un Funko",
        Description = "Actualiza los datos de un Funko existente. Solo accesible para administradores."
    )]
    [SwaggerResponse(200, "Funko actualizado correctamente.", typeof(FunkoDto))]
    [SwaggerResponse(400, "Los datos proporcionados no son válidos.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(404, "Funko no encontrado.")]
    public async Task<ActionResult<FunkoDto>> UpdateFunko(
        [SwaggerParameter("Identificador del Funko.")]
        long id,
        [FromBody] FunkoUpdateRequestDto dto,
        CancellationToken ct)
    {
        var response = await _service.UpdateFunkoAsync(id, dto, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<FunkoDto>();
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPatch("{id}/category")]
    [SwaggerOperation(
        Summary = "Cambiar la categoría de un Funko",
        Description = "Modifica la categoría asociada a un Funko. Solo accesible para administradores."
    )]
    [SwaggerResponse(200, "Categoría actualizada correctamente.", typeof(FunkoDto))]
    [SwaggerResponse(400, "Datos inválidos.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(404, "Funko o categoría no encontrados.")]
    public async Task<ActionResult<FunkoDto>> UpdateCategoryFromFunko(
        [SwaggerParameter("Identificador del Funko.")]
        long id,
        [FromBody] FunkoUpdateCategoryRequestDto dto,
        CancellationToken ct)
    {
        var response = await _service.UpdateCategoryFromAsync(id, dto, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<FunkoDto>();
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPatch("{id}/supplier")]
    [SwaggerOperation(
        Summary = "Cambiar el proveedor de un Funko",
        Description = "Modifica el proveedor asociado a un Funko. Solo accesible para administradores."
    )]
    [SwaggerResponse(200, "Proveedor actualizado correctamente.", typeof(FunkoDto))]
    [SwaggerResponse(400, "Datos inválidos.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(404, "Funko o proveedor no encontrados.")]
    public async Task<ActionResult<FunkoDto>> UpdateSupplierFromFunko(
        [SwaggerParameter("Identificador del Funko.")]
        long id,
        [FromBody] FunkoUpdateSupplierDto request,
        CancellationToken ct)
    {
        var response = await _service.UpdateSupplierFromFunko(
            id, request, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<FunkoDto>();
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPatch("{id}/price")]
    [SwaggerOperation(
        Summary = "Actualizar el precio de un Funko",
        Description = "Modifica el precio de un Funko y actualiza su historial de precios. Solo accesible para administradores."
    )]
    [SwaggerResponse(200, "Precio actualizado correctamente.", typeof(FunkoPriceUpdatedResponseDto))]
    [SwaggerResponse(400, "Precio o datos inválidos.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(404, "Funko no encontrado.")]
    public async Task<ActionResult<FunkoDto>> UpdatePriceFromFunko(
        [SwaggerParameter("Identificador del Funko.")]
        long id,
        [FromBody] FunkoUpdatePriceRequestDto request,
        CancellationToken ct)
    {
        var response = await _service.UpdatePriceAsync(
            id, request, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<FunkoDto>();
    }

    // =========================================================
    // DELETE / RESTORE
    // =========================================================

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Eliminar un Funko",
        Description = "Realiza un borrado lógico del Funko. Solo accesible para administradores."
    )]
    [SwaggerResponse(200, "Funko eliminado correctamente.", typeof(FunkoDeletedResponseDto))]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(404, "Funko no encontrado.")]
    public async Task<ActionResult<FunkoDeletedResponseDto>> DeleteFunko(
        [SwaggerParameter("Identificador del Funko.")]
        long id,
        CancellationToken ct)
    {
         var email = User.FindFirst("email")?.Value;
        var response = await _service.DeleteFunkoAsync(id, email, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<FunkoDeletedResponseDto>();
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPatch("{id}/restore")]
    [SwaggerOperation(
        Summary = "Restaurar un Funko",
        Description = "Restaura un Funko eliminado lógicamente. Solo accesible para administradores."
    )]
    [SwaggerResponse(200, "Funko restaurado correctamente.", typeof(FunkoDto))]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(404, "Funko no encontrado.")]
    public async Task<ActionResult<FunkoDto>> RestoreFunko(
        [SwaggerParameter("Identificador del Funko.")]
        long id,
        CancellationToken ct)
    {
        var response = await _service.RestoreDeletedFunkoAsync(id, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<FunkoDto>();
    }

    // =========================================================
    // UPDATE IMAGE
    // =========================================================

    [Authorize(Roles = "ADMIN")]
    [HttpPost("{id}/image")]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(
        Summary = "Subir imagen de un Funko",
        Description = "Sube o actualiza la imagen asociada a un Funko. Solo accesible para administradores."
    )]
    [SwaggerResponse(200, "Imagen actualizada correctamente.", typeof(FunkoDto))]
    [SwaggerResponse(400, "Archivo inválido.")]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(404, "Funko no encontrado.")]
    public async Task<ActionResult<FunkoDto>> UploadFunkoImage(
        [SwaggerParameter("Identificador del Funko.")]
        long id,
        [SwaggerParameter("Archivo de imagen que se va a subir.")]
        IFormFile file,
        CancellationToken ct)
    {
        var response = await _service.UpdateImageFromFunkoAsync(id, file, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<FunkoDto>();
    }

    // =========================================================
    // PRICE HISTORY (CASSANDRA)
    // =========================================================

    [Authorize(Roles = "ADMIN")]
    [HttpGet("{id}/price-history")]
    [SwaggerOperation(
        Summary = "Consultar historial de precios",
        Description = "Obtiene desde Cassandra el historial de cambios de precio de un Funko. Solo accesible para administradores."
    )]
    [SwaggerResponse(200, "Historial obtenido correctamente.", typeof(FunkoPrieceHistoryResponsesDto))]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(404, "Funko o historial no encontrados.")]
    public async Task<ActionResult<FunkoPrieceHistoryResponsesDto>> GetFunkoPriceHistory(
        [SwaggerParameter("Identificador del Funko.")]
        long id,
        CancellationToken ct)
    {
        var response = await _historyService.GetHistoryAsync(id, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<FunkoPrieceHistoryResponsesDto>();
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id}/historial-precios")]
    [SwaggerOperation(
        Summary = "Eliminar historial de precios",
        Description = "Elimina el historial de precios asociado a un Funko. Solo accesible para administradores."
    )]
    [SwaggerResponse(200, "Historial eliminado correctamente.", typeof(PriceHistoryModifiedResponse))]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(404, "Historial no encontrado.")]
    public async Task<ActionResult<PriceHistoryModifiedResponse>> DeleteFunkoPriceHistoryById(
        [SwaggerParameter("Identificador del Funko.")]
        long id,
        CancellationToken ct)
    {
        var response = await _historyService.DeleteHistoryAsync(id, ct);

        return Ok(response.Value);
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPatch("{id}/historial-precios")]
    [SwaggerOperation(
        Summary = "Restaurar historial de precios",
        Description = "Restaura el historial de precios eliminado de un Funko. Solo accesible para administradores."
    )]
    [SwaggerResponse(200, "Historial restaurado correctamente.", typeof(PriceHistoryModifiedResponse))]
    [SwaggerResponse(401, "No autenticado.")]
    [SwaggerResponse(403, "No tienes permisos de administrador.")]
    [SwaggerResponse(404, "Historial no encontrado.")]
    public async Task<ActionResult<PriceHistoryModifiedResponse>> RestoreFunkoPriceHistoryById(
        [SwaggerParameter("Identificador del Funko.")]
        long id,
        CancellationToken ct)
    {
        var response = await _historyService.RestoreHistoryAsync(id, ct);

        return Ok(response.Value);
    }
}