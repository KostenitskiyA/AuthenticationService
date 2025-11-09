using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using SystemClaimTypes = System.Security.Claims.ClaimTypes;

namespace Application.Services;

public class GoogleService(
    IUserRepository userRepository,
    IGoogleUserRepository googleUserRepository,
    ITokenService tokenService) : IGoogleService
{
    public async Task<User> SignUpAsync(HttpContext context, CancellationToken ct)
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
            user = await userRepository.AddAsync(new User { Name = name, Email = email, HasGoogleAuth = true }, ct);
            await googleUserRepository.AddAsync(new GoogleUser { Id = user.Id, GoogleId = googleId }, ct);
        }
        else
        {
            user.GoogleUser = new GoogleUser { Id = user.Id, GoogleId = googleId };
            user.HasGoogleAuth = true;
        }

        await userRepository.SaveChangesAsync(ct);
        await tokenService.AppendTokensAsync(context, user);
        context.Response.Cookies.Delete($".AspNetCore.{AuthenticationSchemes.GoogleCookie}");

        return user;
    }
}
