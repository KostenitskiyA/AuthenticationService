using System.Net;

namespace Authentication.API.Exceptions;

public class NotAuthorizedException(HttpStatusCode statusCode = HttpStatusCode.Forbidden) : Exception
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}