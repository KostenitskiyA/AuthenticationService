using Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using AuthenticationSchemes = Domain.Enums.AuthenticationSchemes;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoogleController(IUserService userService) : ControllerBase
{
    [HttpGet("login")]
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

    [HttpGet("callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string redirectUrl, CancellationToken ct)
    {
        await userService.GoogleSignUpAsync(HttpContext, ct);

        return Redirect(redirectUrl);
    }
}