public record ResourceNotification<T>(
    T id,
    string resource,
    string method, // no se si es la mejor forma de representarlo
    string status,
    string message
);

