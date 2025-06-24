using System.Net;
using System.Security.Claims;
using Authentication.API.Exceptions;
using Authentication.API.Interfaces;
using Authentication.API.Models.Dtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController(
    ILogger<AuthenticationController> logger,
    IUserService userService
) : ControllerBase
{
    [HttpPost("signup")]
    public async Task SignUp([FromBody] SignInRequest request, CancellationToken ct)
    {
        logger.LogInformation("{Email} try to sign up", request.Email);
        await userService.SignUpAsync(HttpContext, request, ct);
        logger.LogInformation("{Email} sign up", request.Email);
    }

    [HttpPost("login")]
    public async Task LogIn([FromBody] LogInRequest request, CancellationToken ct)
    {
        logger.LogInformation("{Email} try to login", request.Email);
        await userService.LogInAsync(HttpContext, request, ct);
        logger.LogInformation("{Email} login", request.Email);
    }

    [Authorize]
    [HttpDelete("logout")]
    public async Task LogOut(CancellationToken ct)
    {
        await userService.LogOutAsync(HttpContext, ct);
    }

    [Authorize]
    [HttpDelete("delete")]
    public async Task DeleteAsync(CancellationToken ct)
    {
        await userService.DeleteAsync(HttpContext, ct);
    }

    [HttpGet("google/login")]
    public IActionResult LoginWithGoogle()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback), "Authentication")
        };
        
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback(CancellationToken ct)
    {
        var claims = HttpContext.User.Identities.First().Claims.ToList();

        if (!claims.Any())
            throw new DomainException("User not authorized", HttpStatusCode.Unauthorized);
        
        await userService.SignUpAsync(
            HttpContext, 
            new SignInRequest
            {
                Name = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)!.Value,
                Email = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)!.Value
            }, ct);

        return Redirect("http://localhost:5173/login");
    }
}