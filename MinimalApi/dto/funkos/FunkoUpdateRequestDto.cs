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
public record FunkoUpdateRequestDto(
    [param: Required(ErrorMessage = "Nombre requerido")]
    [param: MinLength(2)]
    [param: MaxLength(100)]
    string Name,

    [param: Required(ErrorMessage = "Franquicia requerida")]
    [param: MinLength(1)]
    [param: MaxLength(100)]
    string Franchise,

    [param: Range(1, int.MaxValue)]
    int Number,

    bool Exclusive,

    [param: Range(0.01, 1000000)]
    double Price,
    [param: Required(ErrorMessage = "RowVersion requerido")]
    uint RowVersion
);
