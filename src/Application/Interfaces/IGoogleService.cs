using Domain.Entities;

namespace Application.Interfaces;

public interface IGoogleService
{
    Task<User> SignUpAsync(CancellationToken ct = default);
}
