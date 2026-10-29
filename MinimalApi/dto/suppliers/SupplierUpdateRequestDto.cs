/// <summary>
///     Dto solicitud para actualizar un proveedor.
/// </summary>
/// <param name="Name">Nombre del proveedor.</param>
public record SupplierUpdateRequestDto(
    string Name,
    uint RowVersion
);