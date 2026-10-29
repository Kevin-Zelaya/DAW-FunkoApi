using System.ComponentModel.DataAnnotations;

/// <summary>
///     Dto complementario para crear un funko,
///     desacopla la validaciones del dto original
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="Description"></param>
public record FunkoUpdateCategoryRequestDto(

    [param: Required(ErrorMessage = "Id requerido")]
    Guid Id,
    [param: Required(ErrorMessage = "RowVersion requerido")]
    uint RowVersion
);