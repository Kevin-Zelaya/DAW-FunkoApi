/// <summary>
///     Dto respuesta para información básica de un proveedor.
/// </summary>
/// <param name="Id">Id único para proveedor.</param>
/// <param name="Name">Nombre del proveedor.</param>
public record SupplierDto(
    string Id,
    string Name,
    uint RowVersion
);