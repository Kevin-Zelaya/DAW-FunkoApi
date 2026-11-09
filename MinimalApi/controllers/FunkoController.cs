using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MinimalApi.config;
using MinimalApi.repositories;
using Serilog;

[ApiController]
[Route("api/funkos")]
public class FunkoController(
    IFunkoService _service,
    IFunkoPriceHistoryService _historyService
) : ControllerBase
{ 
    // =========================================================
    // CREATE
    // =========================== 
    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public async Task<ActionResult<FunkoDto>> CreateFunko(
        [FromBody] FunkoCreateRequestDto dto)
    {
        var response = await _service.CreateFunkoAsync(dto);

        return response.IsSuccess
            ? CreatedAtAction(nameof(GetFunkoById),
                new { id = response.Value.Id }, response.Value)
            : response.Error.ToHttpResult<FunkoDto>();
    }

    // =========================================================
    // READ
    // =========================================================

    /// <summary>
    /// Get a Funko by its ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FunkoDto>> GetFunkoById(
        long id,
        CancellationToken ct)
    {
        var funko = await _service.GetFunkoByIdAsync(id, ct);

        return funko.IsSuccess
            ? Ok(funko.Value)
            : funko.Error.ToHttpResult<FunkoDto>();
    }

    /// <summary>
    /// Get all Funkos.
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult<List<FunkoDto>>> GetAllFunkos(
        CancellationToken ct)
    {
        var funkos = await _service.GetAllFunkos(ct);

        return funkos.IsSuccess
            ? Ok(funkos.Value)
            : funkos.Error.ToHttpResult<List<FunkoDto>>();
    }

    /// <summary>
    /// Get paginated Funkos.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PaginatedResouce<FunkoDto>>> GetPaginatedFunkos(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5,
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
    } // implemntar la páginación en todos los endpoints, terminar de probar endpoints de suppliers y hacer los de category
    // testear lo que se tenga y meter el middlerware de excepciones

    // =========================================================
    // UPDATE
    // =========================================================

    /// <summary>
    /// Update a Funko.
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id}")]
    public async Task<ActionResult<FunkoDto>> UpdateFunko(
        long id,
        [FromBody] FunkoUpdateRequestDto dto,
        CancellationToken ct)
    {
        var response = await _service.UpdateFunkoAsync(id, dto, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<FunkoDto>();
    }

    /// <summary>
    /// Update a Funko's category.
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpPatch("{id}/category")]
    public async Task<ActionResult<FunkoDto>> UpdateCategoryFromFunko(
        long id,
        [FromBody] FunkoUpdateCategoryRequestDto dto,
        CancellationToken ct)
    {
        var response = await _service.UpdateCategoryFromAsync(id, dto, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<FunkoDto>();
    }

    /// <summary>
    /// Update a Funko's supplier.
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpPatch("{id}/supplier")] // QUITAR ID
    public async Task<ActionResult<FunkoDto>> UpdateSupplierFromFunko(
        long id,
        FunkoUpdateSupplierDto request,
        CancellationToken ct)
    {
        var response = await _service.UpdateSupplierFromFunko(
            id, request, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<FunkoDto>();
    }
    /// <summary>
    /// Update a Funko's price.
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpPatch("{id}/price")] // QUITAR ID
    public async Task<ActionResult<FunkoDto>> UpdatePriceFromFunko(
        long id,
        FunkoUpdatePriceRequestDto request,
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

    /// <summary>
    /// Soft-delete a Funko.
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<FunkoDeletedResponseDto>> DeleteFunko(
        long id,
        CancellationToken ct)
    {
        var response = await _service.DeleteFunkoAsync(id, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<FunkoDeletedResponseDto>();
    }

    /// <summary>
    /// Restore a deleted Funko.
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpPatch("{id}/restore")]
    public async Task<ActionResult<FunkoDto>> RestoreFunko(
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
    [HttpPost("{id}/image")]
    [Authorize(Roles = "ADMIN")]
    public async Task<ActionResult<FunkoDto>> UploadFunkoImage(
        long id,
        IFormFile file,
        CancellationToken ct)
    {
        var response = await _service.UpdateImageFromFunkoAsync(id, file, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<FunkoDto>();
    }
    // =========================================================
    // HISTORIAL DE PRECIOS (CASSANDRA) - SOLO ADMINS
    // =========================================================

    /// <summary>
    /// Obtiene el historial de cambios de precio de un Funko desde Cassandra (Exclusivo para Admins).
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpGet("{id}/price-history")]
    public async Task<ActionResult<FunkoPrieceHistoryResponsesDto>> GetFunkoPriceHistory(long id, CancellationToken ct)
    {
        var response = await _historyService.GetHistoryAsync(id,ct);


        return response.IsSuccess
            ? Ok(response.Value)
            : response.Error.ToHttpResult<FunkoPrieceHistoryResponsesDto>();
    }
    /// <summary>
    /// Eliminar historial de precios
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpDelete("{id}/historial-precios")]
    public async Task<ActionResult<PriceHistoryModifiedResponse>> DeleteFunkoPriceHistoryById(long id, CancellationToken ct)
    {
        var response = await _historyService.DeleteHistoryAsync(id, ct);


        return Ok(response.Value);
    }
    /// <summary>
    /// Restablece el historial de precios
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpPatch("{id}/historial-precios")]
    public async Task<ActionResult<PriceHistoryModifiedResponse>> RestoreFunkoPriceHistoryById(long id, CancellationToken ct)
    {
        var response = await _historyService.RestoreHistoryAsync(id, ct);


        return Ok(response.Value);
    }
}
