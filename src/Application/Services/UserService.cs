using System.Security.Claims;
using Application.Dtos;
using Application.Interfaces;
using Application.Validators;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using SystemClaimTypes = System.Security.Claims.ClaimTypes;
using AuthenticationSchemes = Domain.Enums.AuthenticationSchemes;

namespace Application.Services;

public class UserService(
    IHttpContextAccessor httpContextAccessor,
    IUserRepository userRepository,
    IGoogleUserRepository googleUserRepository,
    ITokenService tokenService,
    IPasswordHasher passwordHasher) : IUserService
{
    public async Task<User> SignUpAsync(SignInRequest request, CancellationToken ct)
    {
        var validator = new SignInRequestValidator(userRepository);
        await validator.ValidateAndThrowAsync(request, ct);

        var user = await userRepository.AddAsync(
            new User
            {
                Email = request.Email.Trim()
                    .ToLowerInvariant(),
                Name = request.Name,
                PasswordHash = !string.IsNullOrEmpty(request.Password)
                    ? passwordHasher.HashPassword(request.Password)
                    : string.Empty
            },
            ct
        );

        await userRepository.SaveChangesAsync(ct);
        await tokenService.AppendTokensAsync(user);

        return user;
    }

    public async Task<User> GoogleSignUpAsync(CancellationToken ct)
    {
        var httpContext = httpContextAccessor.HttpContext!;
        var result = await httpContext.AuthenticateAsync(AuthenticationSchemes.Google);

        if (!result.Succeeded || result.Principal is null)
            throw new DomainException("Google authentication failed");

        var claims = result.Principal.Claims.ToList();
        var googleId = claims.FirstOrDefault(claim => claim.Type == SystemClaimTypes.NameIdentifier)?.Value;
        var name = claims.FirstOrDefault(claim => claim.Type == SystemClaimTypes.Name)?.Value;
        var email = claims.FirstOrDefault(claim => claim.Type == SystemClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
            throw new DomainException("Google authentication failed");

        var user = await userRepository.GetByEmailAsync(email, ct);

        if (user is null)
        {
            user = await userRepository.AddAsync(new User { Name = name, Email = email, HasGoogleAuth = true }, ct);

            await googleUserRepository.AddAsync(new GoogleUser { Id = user.Id, GoogleId = googleId }, ct);
        }
        else
        {
            if (user.GoogleUser is null)
            {
                await googleUserRepository.AddAsync(new GoogleUser { Id = user.Id, GoogleId = googleId }, ct);
            }
            else
            {
                user.GoogleUser.GoogleId = googleId;
                googleUserRepository.Update(user.GoogleUser);
            }

            user.HasGoogleAuth = true;
            userRepository.Update(user);
        }

        await userRepository.SaveChangesAsync(ct);
        await tokenService.AppendTokensAsync(user);
        httpContext.Response.Cookies.Delete($".AspNetCore.{AuthenticationSchemes.GoogleCookie}");

        return user;
    }

    public async Task<User> LogInAsync(LogInRequest request, CancellationToken ct)
    {
        var validator = new LogInRequestValidator();
        await validator.ValidateAndThrowAsync(request, ct);

        var user = await userRepository.GetByEmailAsync(request.Email, ct);

        if (user is null)
            throw new DomainException($"User {request.Email} not found");

        if (string.IsNullOrEmpty(user.PasswordHash) || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new DomainException($"User {request.Email} password invalid");

        await tokenService.RevokeTokensAsync();
        await tokenService.AppendTokensAsync(user);

        return user;
    }

    public async Task DeleteAsync(CancellationToken ct)
    {
        var httpContext = httpContextAccessor.HttpContext!;
        var email = httpContext.User.FindFirstValue(SystemClaimTypes.Email);

        if (httpContext.User.Identity is not { IsAuthenticated: true } || string.IsNullOrEmpty(email))
            throw new NotAuthorizedException();

        var user = await userRepository.GetByEmailAsync(email, ct);

        if (user is null)
            throw new EntityNotFoundException(nameof(User), email);

        userRepository.Delete(user);

        if (user is { HasGoogleAuth: true, GoogleUser: not null })
            googleUserRepository.Delete(user.GoogleUser);

        await userRepository.SaveChangesAsync(ct);
        await tokenService.RevokeTokensAsync();
    }
}
