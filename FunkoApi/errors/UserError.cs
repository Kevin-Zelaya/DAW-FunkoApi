public static class UserError
{
    public static NotFoundError NotFound(long id) => NotFoundError.FromResource("User", id.ToString());
    public static NotFoundError NotFound(string email) => NotFoundError.FromResource("User", email);
    public static ConflictError NombreDuplicado(string nombre) => ConflictError.Duplicate("User", nombre);
}