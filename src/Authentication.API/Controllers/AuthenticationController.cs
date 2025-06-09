using System.Security.Claims;
using Authentication.API.Interfaces;
using Authentication.API.Models.Dtos;
using Microsoft.AspNetCore.Authentication;
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

    [HttpGet("login-google")]
    public IActionResult LoginWithGoogle([FromQuery] string redirectUrl = "/")
    {
        var url = Url.Action(nameof(GoogleCallback), "Authentication", new { redirectUrl });
        var properties = new AuthenticationProperties
        {
            RedirectUri = url
        };
        
        return Challenge(properties, "GoogleToken");
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string redirectUrl = "/")
    {
        var result = await HttpContext.AuthenticateAsync();

        if (!result.Succeeded || result.Principal == null)
        {
            return Unauthorized();
        }

        var claims = result.Principal.Identities.First().Claims;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

        return Redirect(redirectUrl);
    }
}