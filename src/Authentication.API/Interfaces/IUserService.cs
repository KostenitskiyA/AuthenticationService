using Authentication.API.Models.Dtos;
using Authentication.API.Models.Entities;

namespace Authentication.API.Interfaces;

public interface IUserService
{
    Task<User> SignUpAsync(HttpContext context, SignInRequest request, CancellationToken ct = default);

    Task<User> GoogleSignUpAsync(HttpContext context, CancellationToken ct = default);
    
    Task<User> LogInAsync(HttpContext context, LogInRequest request, CancellationToken ct = default);

    Task LogOutAsync(HttpContext context, CancellationToken ct = default);

    Task DeleteAsync(HttpContext context, CancellationToken ct = default);

    Task RefreshAsync(HttpContext context, CancellationToken ct = default);
}