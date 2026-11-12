/// <summary>
///     Dto para transporte de categoria
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="Description"></param>
public record CategoryDto(
    string Id,
    string Name,
    string Description,
    uint RowVersion

);