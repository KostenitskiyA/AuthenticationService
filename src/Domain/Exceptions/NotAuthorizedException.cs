using System.Net;

namespace Domain.Exceptions;

public class NotAuthorizedException(HttpStatusCode statusCode = HttpStatusCode.Unauthorized) : Exception
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}