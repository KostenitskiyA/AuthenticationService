using Application.Dtos;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IUserService
{
    Task<User> SignUpAsync(HttpContext context, SignInRequest request, CancellationToken ct = default);

    Task<User> GoogleSignUpAsync(HttpContext context, CancellationToken ct = default);

    Task<User> LogInAsync(HttpContext context, LogInRequest request, CancellationToken ct = default);

    Task DeleteAsync(HttpContext context, CancellationToken ct = default);
}
