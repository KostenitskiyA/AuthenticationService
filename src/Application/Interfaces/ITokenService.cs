using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface ITokenService
{
    Task AuthenticationAsync(HttpContext context, User user);
}