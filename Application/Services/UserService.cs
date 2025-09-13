using System.Security.Claims;
using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Redis.Interfaces;
using SystemClaimTypes = System.Security.Claims.ClaimTypes;

namespace Application.Services;

public class UserService(
    IUserRepository userRepository,
    IGoogleUserRepository googleUserRepository,
    ITokenService tokenService,
    IRedisService redisService) 
    : IUserService
{
    public async Task<User> SignUpAsync(HttpContext context, SignInRequest request, CancellationToken ct)
    {
        var validator = new SignInRequestValidator(userRepository);
        await validator.ValidateAndThrowAsync(request, ct);
        
        var user = await userRepository.AddAsync(
            new User
            {
                Email = request.Email.Trim().ToLowerInvariant(),
                Name = request.Name,
                PasswordHash = !string.IsNullOrEmpty(request.Password)
                    ? BCrypt.Net.BCrypt.HashPassword(request.Password)
                    : string.Empty
            }, 
            ct);
        await userRepository.SaveChangesAsync(ct);

        await tokenService.AuthenticationAsync(context, user);

        return user;
    }

    public async Task<User> GoogleSignUpAsync(HttpContext context, CancellationToken ct)
    {
        var result = await context.AuthenticateAsync(AuthenticationSchemes.Google);

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
            user = await userRepository.AddAsync(
                new User
                {
                    Name = name,
                    Email = email,
                    HasGoogleAuth = true
                }, ct);

            await googleUserRepository.AddAsync(
                new GoogleUser
                {
                    Id = user.Id,
                    GoogleId = googleId
                }, ct);

            await userRepository.SaveChangesAsync(ct);
        }
        else
        {
            user.GoogleUser = new GoogleUser { Id = user.Id, GoogleId = googleId };
            user.HasGoogleAuth = true;
            await userRepository.SaveChangesAsync(ct);
        }

        await tokenService.AuthenticationAsync(context, user);
        context.Response.Cookies.Delete($".AspNetCore.{AuthenticationSchemes.GoogleCookie}");

        return user;
    }

    public async Task<User> LogInAsync(HttpContext context, LogInRequest request, CancellationToken ct)
    {
        var validator = new LogInRequestValidator(userRepository);
        await validator.ValidateAndThrowAsync(request, ct);
        
        var user = await userRepository.GetByEmailAsync(request.Email, ct);

        if (user is null)
            throw new DomainException($"User {request.Email} not found");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new DomainException($"User {request.Email} password invalid");

        await tokenService.AuthenticationAsync(context, user);

        return user;
    }

    public async Task LogOutAsync(HttpContext context, CancellationToken ct)
    {
        context.Response.Cookies.Delete(AuthenticationSchemes.Token);
        context.Response.Cookies.Delete(AuthenticationSchemes.RefreshToken);

        await Task.CompletedTask;
    }

    public async Task DeleteAsync(HttpContext context, CancellationToken ct)
    {
        var email = context.User.FindFirstValue(SystemClaimTypes.Email);

        if (context.User.Identity is not { IsAuthenticated: true } || string.IsNullOrEmpty(email))
            throw new NotAuthorizedException();

        var user = await userRepository.GetByEmailAsync(email, ct);

        if (user is null)
            throw new EntityNotFoundException(nameof(User), email);

        userRepository.Delete(user);
        
        if (user.HasGoogleAuth)
            googleUserRepository.Delete(user.GoogleUser);
        
        await userRepository.SaveChangesAsync(ct);

        await LogOutAsync(context, ct);
    }

    public async Task RefreshAsync(HttpContext context, CancellationToken ct)
    {
        var refreshToken = context.Request.Cookies[AuthenticationSchemes.RefreshToken];

        if (string.IsNullOrEmpty(refreshToken))
            return;

        var id = await redisService.GetStringAsync<string>($"authentication:refresh:{refreshToken}");

        if (string.IsNullOrEmpty(id))
        {
            await LogOutAsync(context, ct);
            return;
        }
        
        if (Guid.TryParse(id, out var userId))
        {
            var user = await userRepository.GetByIdAsync(userId, ct);
            
            if (user is null)
                throw new EntityNotFoundException(nameof(User), id);
            
            await tokenService.AuthenticationAsync(context, user);
        }
    }
}