using System.Net;

namespace Domain.Exceptions;

public class EntityNotFoundException(string entity, string value, HttpStatusCode statusCode = HttpStatusCode.NotFound)
    : Exception($"{entity} with value '{value}' was not found.")
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}
