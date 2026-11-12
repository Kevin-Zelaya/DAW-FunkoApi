/// <summary>
///     Dto que permite páginar 
///     recursos con una lista 
///     generica.
/// </summary>
/// <typeparam name="T">Cualquier tipo</typeparam>
/// <param name="Items">Lista de entidades</param>
/// <param name="Count">Cantidad de elementos en la lista</param>
/// <param name="TotalPages">Total de páginas</param>
/// <param name="HasPreviusPage">¿Tiene página anterior?</param>
/// <param name="HasNextPage">¿Tiene página siguiente?</param>
public record PaginatedResouce<T>(
    List<T> Items,
    int Count,
    int TotalPages,
    bool HasPreviusPage,
    bool HasNextPage
);