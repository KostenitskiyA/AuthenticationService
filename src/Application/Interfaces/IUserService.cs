using Application.Dtos;
using Domain.Entities;

namespace Application.Interfaces;

public interface IUserService
{
    Task<User> SignUpAsync(SignInRequest request, CancellationToken ct = default);

    Task<User> LogInAsync(LogInRequest request, CancellationToken ct = default);

    Task DeleteAsync(CancellationToken ct = default);
}
