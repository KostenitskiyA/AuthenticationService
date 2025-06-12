using System.Security.Claims;
using Authentication.API.Exceptions;
using Authentication.API.Interfaces;
using Authentication.API.Models.Dtos;
using Authentication.API.Models.Entities;
using Authentication.API.Models.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Authentication.API.Services;

public class UserService(
    IUserRepository userRepository,
    IOptionsMonitor<AuthenticationConfiguration> authenticationConfigurationMonitor
) : IUserService
{
    private readonly AuthenticationConfiguration _authenticationConfiguration = 
        authenticationConfigurationMonitor.CurrentValue;

    private ClaimsPrincipal CreateUserClaims(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email)
        };
        var claimsIdentity = new ClaimsIdentity(claims, _authenticationConfiguration.AuthenticationOptions.TokenName);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        return claimsPrincipal;
    }

    public async Task<User> SignUpAsync(HttpContext context, SignInRequest request, CancellationToken ct)
    {
        var isUserExists = await userRepository.IsExistsAsync(user => user.Email == request.Email, ct);

        if (isUserExists)
            throw new DomainException($"User {request.Email} already exist");

        var user = await userRepository.AddAsync(
            new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = !string.IsNullOrEmpty(request.Password) 
                    ? BCrypt.Net.BCrypt.HashPassword(request.Password) 
                    : string.Empty
            }, ct);

        await context.SignInAsync(CreateUserClaims(user));

        return user;
    }

    public async Task<User> LogInAsync(HttpContext context, LogInRequest request, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, ct);

        if (user is null)
            throw new DomainException($"User {request.Email} not found");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            throw new DomainException($"User {request.Email} password invalid");

        await context.SignInAsync(CreateUserClaims(user));

        return user;
    }

    public async Task LogOutAsync(HttpContext context, CancellationToken ct)
    {
        await context.SignOutAsync();
    }

    public async Task DeleteAsync(HttpContext context, CancellationToken ct)
    {
        var email = context.User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(email))
            throw new DomainException("User not authorized");

        var user = await userRepository.GetByEmailAsync(email, ct);

        if (user is null)
            throw new DomainException($"User {email} not found");

        await userRepository.DeleteAsync(user, ct);
        await LogOutAsync(context, ct);
    }
}