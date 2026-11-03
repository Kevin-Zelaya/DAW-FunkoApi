public static class AuthError
{
    public static UnauthorizedError InvalidCredentials() => UnauthorizedError.InvalidCredentials();
    public static ConflictError DuplicateUser(string email) => ConflictError.Duplicate("usuario", email);
}