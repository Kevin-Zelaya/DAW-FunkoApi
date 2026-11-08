public interface INotificationService
{
    /// <summary>
    /// Generico, permite notificar a los administradores
    /// con un mismo metodo de un recurso actualizado
    /// </summary>
    /// <typeparam name="T">Tipo de la llave primaria del recurso</typeparam>
    /// <param name="notification">Dto formado para el caso</param>
    /// <see cref="ResourceNotification{T}"/>
    /// <returns></returns>
    Task NotifyResourceChangedAsync<T>(ResourceNotification<T> notification);
}