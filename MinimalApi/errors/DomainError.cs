using System.Net;

public record  DomainError(string Message, HttpStatusCode StatusCode);

public record NotFoundError(string Message, HttpStatusCode StatusCode) : DomainError(Message, StatusCode)
{
    public static NotFoundError FromResource(string resource, string id) =>
        new($"No existe {resource} con id {id}", HttpStatusCode.NotFound);
}

public sealed record ConflictError(string Message, HttpStatusCode StatusCode) : DomainError(Message, StatusCode)
{
    public static ConflictError Duplicate(string resourceType, string value) =>
        new($"Ya existe un {resourceType} con el valor '{value}'", HttpStatusCode.Conflict);
}

public sealed record BusinessRuleError(string Message) : DomainError(Message, HttpStatusCode.BadRequest);

public sealed record InternalError(string Message) : DomainError(Message, HttpStatusCode.InternalServerError);