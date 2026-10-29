/// <summary>
/// Dto para transporte de funko
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="Franchise"></param>
/// <param name="Number"></param>
/// <param name="SupplierId"></param>
/// <param name="Category"></param>
/// <param name="exclusivom"></param>
/// <param name="precio"></param>
public record FunkoDto(
    long Id,
    string Name,
    string Franchise,
    int Number,
    CategoryDto Category,
    bool Exclusive,
    double Price,
    SupplierDto Supplier,
    uint RowVersion

);


