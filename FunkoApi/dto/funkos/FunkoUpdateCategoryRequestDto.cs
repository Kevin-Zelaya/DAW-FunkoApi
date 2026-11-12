using System.ComponentModel.DataAnnotations;

/// <summary>
///     Dto complementario para crear un funko,
///     desacopla la validaciones del dto original
/// </summary>
/// <param name="SupplierId"></param>
/// <param name="Name"></param>
/// <param name="Description"></param>
public record FunkoUpdateCategoryRequestDto(

    [param: Required(ErrorMessage = "Id requerido")]
    [param: YoutubeId]
    string CategoryId,
    [param: Required(ErrorMessage = "RowVersion requerido")]
    uint RowVersion
);