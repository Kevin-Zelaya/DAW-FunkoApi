/// <summary>
/// Define las operaciones para gestionar la caché de la aplicación.
/// </summary>
public interface ICache
{
    /// <summary>
    /// Elimina todos los elementos almacenados en la caché.
    /// </summary>
    Task ClearAsync();

    /// <summary>
    /// Obtiene un elemento de la caché por su clave.
    /// </summary>
    /// <typeparam name="T">Tipo de dato que se desea obtener.</typeparam>
    /// <param name="key">Clave del elemento almacenado.</param>
    /// <returns>Elemento encontrado o null si no existe.</returns>
    Task<T?> GetTAsync<T>(string key);

    /// <summary>
    /// Elimina un elemento de la caché por su clave.
    /// </summary>
    /// <param name="key">Clave del elemento que se desea eliminar.</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// Almacena un elemento en la caché.
    /// </summary>
    /// <typeparam name="T">Tipo de dato que se desea almacenar.</typeparam>
    /// <param name="key">Clave con la que se almacenará el elemento.</param>
    /// <param name="value">Valor que se desea guardar.</param>
    /// <param name="expiration">Tiempo de expiración opcional del elemento.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
}