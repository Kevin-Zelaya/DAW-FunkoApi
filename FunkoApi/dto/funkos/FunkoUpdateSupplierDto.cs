using System.ComponentModel.DataAnnotations;

public record FunkoUpdateSupplierDto(
    [param: Required(ErrorMessage = "Id requerido")]
    [param: ObjectId]
    string SupplierId,
    [param: Required(ErrorMessage = "RowVersion requerido")]
    uint RowVersion
);
