/// <summary>
/// Configuración de la API
/// </summary>
public class ApiConfig
{
    public string Nombre { get; set; } = "Productos EF Core API";
    public string Mensaje { get; set; } = "CRUD con Entity Framework Core + PostgreSQL";
    public bool shouldRecreateDb {get; set;} = false;
}