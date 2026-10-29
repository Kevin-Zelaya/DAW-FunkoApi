using System.ComponentModel.DataAnnotations;

public record FunkoUpdateSupplierDto(
    [param: Required(ErrorMessage = "Id requerido")]
    string Id,
    [param: Required(ErrorMessage = "RowVersion requerido")]
    uint RowVersion
);
