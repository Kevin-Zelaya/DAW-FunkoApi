using System.ComponentModel.DataAnnotations;

public record CategoryUpdateRequestDto
(
    [param: Required(ErrorMessage = "Nombre requerido")]
    [param: MinLength(2)]
    [param: MaxLength(100)]
    string Name,
    [param: Required(ErrorMessage = "Descripción requerido")]
    [param: MinLength(2)]
    [param: MaxLength(100)]
    string Description,
    [param: Required(ErrorMessage = "RowVersion requerido")]
    uint RowVersion
);