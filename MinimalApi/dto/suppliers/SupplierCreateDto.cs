using System.ComponentModel.DataAnnotations;

public record SupplierCreateRequestDto
(
    [param: Required(ErrorMessage = "Nombre requerido")]
    [param: MinLength(2)]
    [param: MaxLength(100)]
    string Name
);