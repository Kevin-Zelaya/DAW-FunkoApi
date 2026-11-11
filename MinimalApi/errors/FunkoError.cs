public static class FunkoError
{
    public static NotFoundError NotFound(long id) => NotFoundError.FromResource("Funko", id.ToString());
    public static NotFoundError NotFound(string email) => NotFoundError.FromResource("Funko", email);
    public static ConflictError NombreDuplicado(string nombre) => ConflictError.Duplicate("Funko", nombre);
}