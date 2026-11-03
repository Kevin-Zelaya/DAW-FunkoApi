using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MinimalApi.config;
using MinimalApi.repositories;

[ApiController]
[Route("api/[controller]")]
public class FunkoController : ControllerBase
{
    private readonly IFunkoService _service;
    private readonly IFunkoPriceHistoryRepository _priceHistoryRepository; // <--- 1. Declara la variable aquí

    // 2. Inyéctala en el constructor
    public FunkoController(
        IFunkoService service,
        IFunkoPriceHistoryRepository priceHistoryRepository)
    {
        _service = service;
        _priceHistoryRepository = priceHistoryRepository;
    }
    // =========================================================
    // CREATE
    // =========================== 
    [HttpPost]
    public async Task<ActionResult<FunkoDto>> CreateFunko(
        [FromBody] FunkoCreateRequestDto dto)
    {
        var response = await _service.CreateFunkoAsync(dto);

        return response.IsSuccess
            ? CreatedAtAction(nameof(GetFunkoById),
                new { id = response.Value.Id }, response.Value)
            : BadRequest(response.Error);
    }

    // =========================================================
    // READ
    // =========================================================

    /// <summary>
    /// Get a Funko by its ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetFunkoById(
        long id,
        CancellationToken ct)
    {
        var funko = await _service.GetFunkoByIdAsync(id, ct);

        return funko.IsSuccess
            ? Ok(funko.Value)
            : NotFound(funko.Error);
    }

    /// <summary>
    /// Get all Funkos.
    /// </summary>
    [HttpGet("all")]
    public async Task<ActionResult> GetAllFunkos(
        CancellationToken ct)
    {
        var funkos = await _service.GetAllFunkos(ct);

        return funkos.IsSuccess
            ? Ok(funkos.Value)
            : BadRequest(funkos.Error);
    }

    /// <summary>
    /// Get paginated Funkos.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetPaginatedFunkos(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5,
        CancellationToken ct = default)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest("La página y el tamaño deben ser mayores que 0.");

        var funkos = await _service.GetPaginatedFunkos(
            pageNumber, pageSize, ct);

        return funkos.IsSuccess
            ? Ok(funkos.Value)
            : BadRequest(funkos.Error);
    } // implemntar la páginación en todos los endpoints, terminar de probar endpoints de suppliers y hacer los de category
    // testear lo que se tenga y meter el middlerware de excepciones

    // =========================================================
    // UPDATE
    // =========================================================

    /// <summary>
    /// Update a Funko.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<FunkoDto>> UpdateFunko(
        long id,
        [FromBody] FunkoUpdateRequestDto dto,
        CancellationToken ct)
    {
        var response = await _service.UpdateFunkoAsync(id, dto, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : NotFound(response.Error);
    }

    /// <summary>
    /// Update a Funko's category.
    /// </summary>
    [HttpPatch("{id}/category")]
    public async Task<ActionResult<FunkoDto>> UpdateCategoryFromFunko(
        long id,
        [FromBody] FunkoUpdateCategoryRequestDto dto,
        CancellationToken ct)
    {
        var response = await _service.UpdateCategoryFromAsync(id, dto, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : NotFound(response.Error);
    }

    /// <summary>
    /// Update a Funko's supplier.
    /// </summary>
    [HttpPatch("{id}/supplier/{supplierId}")]
    public async Task<ActionResult<FunkoDto>> UpdateSupplierFromFunko(
        long id,
        FunkoUpdateSupplierDto request,
        CancellationToken ct)
    {
        var response = await _service.UpdateSupplierFromFunko(
            id, request, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : NotFound(response.Error);
    }

    // =========================================================
    // DELETE / RESTORE
    // =========================================================

    /// <summary>
    /// Soft-delete a Funko.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteFunko(
        long id,
        CancellationToken ct)
    {
        var response = await _service.DeleteFunkoAsync(id, ct);

        return response.IsSuccess
            ? NoContent()
            : NotFound(response.Error);
    }

    /// <summary>
    /// Restore a deleted Funko.
    /// </summary>
    [HttpPatch("{id}/restore")]
    public async Task<ActionResult<FunkoDto>> RestoreFunko(
        long id,
        CancellationToken ct)
    {
        var response = await _service.RestoreDeletedFunkoAsync(id, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : NotFound(response.Error);
    }

    // =========================================================
    // UPDATE IMAGE
    // =========================================================
    [HttpPost("{id}/image")]
    public async Task<ActionResult> UploadFunkoImage(
        long id,
        IFormFile file,
        CancellationToken ct)
    {
        var response = await _service.UpdateImageFromFunkoAsync(id, file, ct);

        return response.IsSuccess
            ? Ok(response.Value)
            : BadRequest(response.Error);
    }
    // =========================================================
    // HISTORIAL DE PRECIOS (CASSANDRA) - SOLO ADMINS
    // =========================================================

    /// <summary>
    /// Obtiene el historial de cambios de precio de un Funko desde Cassandra (Exclusivo para Admins).
    /// </summary>
    [HttpGet("{id}/historial-precios")]
    [Authorize(Roles = "Admin")]
    [Route("api/funkos")]
    public async Task<ActionResult> GetFunkoPriceHistory(long id)
    {
        var historial = await _priceHistoryRepository.ObtenerHistorialPorFunkoAsync(id);

        if (historial == null || !historial.Any())
        {
            return NotFound(new { mensaje = $"No se encontró historial de precios para el Funko con ID {id}." });
        }

        return Ok(historial);
    }
 // <--- Esta es la ÚLTIMA llave del archivo (cierra la clase FunkoController)
}
