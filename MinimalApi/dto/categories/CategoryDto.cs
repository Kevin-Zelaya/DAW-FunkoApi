/// <summary>
///     Dto para transporte de categoria
/// </summary>
/// <param name="Id"></param>
/// <param name="Name"></param>
/// <param name="Description"></param>
public record CategoryDto(
    Guid Id,
    string Name,
    string Description
);