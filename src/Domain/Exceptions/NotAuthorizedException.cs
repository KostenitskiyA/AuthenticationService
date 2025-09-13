using System.Net;

namespace Domain.Exceptions;

public class NotAuthorizedException(HttpStatusCode statusCode = HttpStatusCode.Forbidden) : Exception
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}