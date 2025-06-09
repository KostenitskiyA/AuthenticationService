using Authentication.API.Models.Dtos;
using Authentication.API.Models.Entities;

namespace Authentication.API.Interfaces;

public interface IUserService
{
    Task<User> SignUpAsync(HttpContext context, SignInRequest request, CancellationToken cancellationToken = default);
    
    Task<User> LogInAsync(HttpContext context, LogInRequest request, CancellationToken cancellationToken = default);
    
    Task LogOutAsync(HttpContext context, CancellationToken cancellationToken = default);

    Task DeleteAsync(HttpContext context, CancellationToken cancellationToken = default);
}