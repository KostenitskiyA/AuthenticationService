using Authentication.API.Interfaces;
using Authentication.API.Models.Dtos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using AuthenticationSchemes = Authentication.API.Models.Enums.AuthenticationSchemes;

namespace Authentication.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController(IUserService userService) : ControllerBase
{
    [HttpPost("signup")]
    public async Task<IActionResult> SignUp([FromBody] SignInRequest request, CancellationToken ct)
    {
        await userService.SignUpAsync(HttpContext, request, ct);
        
        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> LogIn([FromBody] LogInRequest request, CancellationToken ct)
    {
        await userService.LogInAsync(HttpContext, request, ct);
        
        return Ok();
    }

    [Authorize]
    [HttpDelete("logout")]
    public async Task<IActionResult> LogOut(CancellationToken ct)
    {
        await userService.LogOutAsync(HttpContext, ct);

        return Ok();
    }

    [Authorize]
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteAsync(CancellationToken ct)
    {
        await userService.DeleteAsync(HttpContext, ct);
        
        return Ok();
    }
    
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        await userService.RefreshAsync(HttpContext, ct);

        return Ok();
    }

    [HttpGet("google/login")]
    public IActionResult LoginWithGoogle([FromQuery] string redirectUrl)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = QueryHelpers.AddQueryString(
                Url.Action(nameof(GoogleCallback), "Authentication")!, 
                "redirectUrl",
                redirectUrl)
        };
        
        return Challenge(properties, AuthenticationSchemes.Google);
    }
    
    [HttpGet("google/callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string redirectUrl, CancellationToken ct)
    {
        await userService.GoogleSignUpAsync(HttpContext, ct);
        
        return Redirect(redirectUrl);
    }
}