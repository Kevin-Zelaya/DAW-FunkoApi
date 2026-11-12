using System.ComponentModel.DataAnnotations;

/// <summary>
///     Dto para actualización casi completa,
///     deja fuera la categoria para poder actualizarse 
///     por medio del patch
/// </summary>
/// <param name="Name"></param>
/// <param name="Franchise"></param>
/// <param name="Number"></param>
/// <param name="SupplierId"></param>
/// <param name="Exclusive"></param>
/// <param name="Price"></param>
public record FunkoUpdatePriceRequestDto(
    [param: Range(0.01, 1000000)]
    double Price,
    [param: Required(ErrorMessage = "RowVersion requerido")]
    uint RowVersion
);
