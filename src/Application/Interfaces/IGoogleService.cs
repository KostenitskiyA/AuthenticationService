using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IGoogleService
{
    Task<User> SignUpAsync(HttpContext context, CancellationToken ct = default);
}
