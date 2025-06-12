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
    public async Task SignUp([FromBody] SignInRequest request, [FromQuery] string redirectUrl, CancellationToken ct)
    {
        logger.LogInformation("{Email} try to sign up", request.Email);
        await userService.SignUpAsync(HttpContext, request, ct);
        logger.LogInformation("{Email} sign up", request.Email);

        Redirect(redirectUrl);
    }

    [HttpPost("login")]
    public async Task LogIn([FromBody] LogInRequest request, [FromQuery] string redirectUrl, CancellationToken ct)
    {
        logger.LogInformation("{Email} try to login", request.Email);
        await userService.LogInAsync(HttpContext, request, ct);
        logger.LogInformation("{Email} login", request.Email);

        Redirect(redirectUrl);
    }

    [Authorize]
    [HttpDelete("logout")]
    public async Task LogOut([FromQuery] string redirectUrl, CancellationToken ct)
    {
        await userService.LogOutAsync(HttpContext, ct);

        Redirect(redirectUrl);
    }

    [Authorize]
    [HttpDelete("delete")]
    public async Task DeleteAsync([FromQuery] string redirectUrl, CancellationToken ct)
    {
        await userService.DeleteAsync(HttpContext, ct);

        Redirect(redirectUrl);
    }

    [HttpGet("google/login")]
    public IActionResult LoginWithGoogle([FromQuery] string redirectUrl)
    {
        var request = HttpContext.Request;
        
        var url = $"{request.Scheme}://" + 
                  $"{request.Host}" + 
                  $"{Url.Action(nameof(GoogleCallback), "Authentication", new { redirectUrl })}";
        var properties = new AuthenticationProperties { RedirectUri = url };
        
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string redirectUrl, CancellationToken ct)
    {
        var claims = HttpContext.User.Identities.First().Claims.ToList();

        if (!claims.Any())
            throw new DomainException("User not authorized", HttpStatusCode.Unauthorized);

        var name = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name)!.Value;
        var email = claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email)!.Value;
        
        await userService.SignUpAsync(
            HttpContext, 
            new SignInRequest
            {
                Name = name,
                Email = email
            }, ct);

        return Redirect(redirectUrl);
    }
}