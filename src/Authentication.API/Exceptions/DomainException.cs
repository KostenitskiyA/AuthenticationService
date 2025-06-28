using System.Net;

namespace Authentication.API.Exceptions;

public class DomainException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}