public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpeiresIn,
    long UserId,
    string Username,
    string Role
);