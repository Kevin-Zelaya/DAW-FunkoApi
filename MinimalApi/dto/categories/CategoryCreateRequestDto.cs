using System.ComponentModel.DataAnnotations;

/// <summary>
///     Dto complementario para crear un funko,
///     desacopla la validaciones del dto original
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="Description"></param>
public record CategoryCreateRequestDto(

    [param: Required(ErrorMessage = "Id requerido")]
    Guid Id,
    [param: Required(ErrorMessage = "Nombre requerido")]
    [param: MinLength(2)]
    [param: MaxLength(100)]
    string Name,
    [param: Required(ErrorMessage = "Descripción requerido")]
    [param: MinLength(2)]
    [param: MaxLength(100)]
    string Description
);