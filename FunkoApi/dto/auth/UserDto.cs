
/// <summary>
/// Para la representación del usuario
/// y futura modificación
/// </summary>
/// <param name="Id"></param>
/// <param name="Username"></param>
/// <param name="Email"></param>
/// <param name="Role"></param>
/// <param name="IsDeleted"></param>
/// <param name="CreatedAt"></param>
/// <param name="LastLoginAt"></param>
public record UserDto(
    long Id,
    string Username,
    string Email,
    string Role,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);
