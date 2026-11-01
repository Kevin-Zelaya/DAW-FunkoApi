using System.ComponentModel.DataAnnotations;

/// <summary>
///     Dto solicitud para actualizar un proveedor.
/// </summary>
/// <param name="Name">Nombre del proveedor.</param>
public record SupplierUpdateRequestDto(
    [param: Required(ErrorMessage = "Nombre requerido")]
    [param: MinLength(2)]
    [param: MaxLength(100)]
    string Name,
    [param: Required(ErrorMessage = "RowVersion requerido")]
    uint RowVersion
);