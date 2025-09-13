using System.Net;

namespace Domain.Exceptions;

public class DomainException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}