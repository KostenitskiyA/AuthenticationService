using System.Net;

namespace API.Models;

public sealed record Error(HttpStatusCode Code, string Message, ValidationError[] Details);