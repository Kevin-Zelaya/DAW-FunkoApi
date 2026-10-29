using System.ComponentModel.DataAnnotations;

/// <summary>
///     Dto para crear un funko
///     Conteine un dto para la categoría 
///     diferente al dto original, para desacoplar
///     las validacioens
/// </summary>
/// <param name="Name"></param>
/// <param name="Franchise"></param>
/// <param name="Number"></param>
/// <param name="SupplierId"></param>
/// <param name="Category"></param>
/// <param name="Exclusive"></param>
/// <param name="Price"></param>
public record FunkoCreateRequestDto(
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

    [param: Required(ErrorMessage = "Id del proveedor requerida")]
    string SupplierId,

    [param: Required(ErrorMessage = "Categoría requerida")]
    Guid CategoryId,

    bool Exclusive,

    [param: Range(0.01, 1000000)]
    double Price
);
