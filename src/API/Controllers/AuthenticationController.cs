using API.Models;
using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using AuthenticationSchemes = Domain.Enums.AuthenticationSchemes;
using Results = API.Models.Results;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController(IUserService userService, ITokenService tokenService) : ControllerBase
{
    [HttpPost("signup")]
    public async Task<ActionResult<Result>> SignUp([FromBody] SignInRequest request, CancellationToken ct)
    {
        await userService.SignUpAsync(HttpContext, request, ct);

        return Ok(Results.Ok(HttpContext));
    }

    [HttpPost("login")]
    public async Task<ActionResult<Result>> LogIn([FromBody] LogInRequest request, CancellationToken ct)
    {
        await userService.LogInAsync(HttpContext, request, ct);

        return Ok(Results.Ok(HttpContext));
    }

    [Authorize]
    [HttpDelete("logout")]
    public async Task<ActionResult<Result>> LogOut(CancellationToken ct)
    {
        await tokenService.RevokeTokensAsync(HttpContext);

        return Ok(Results.Ok(HttpContext));
    }

    [Authorize]
    [HttpDelete("delete")]
    public async Task<ActionResult<Result>> DeleteAsync(CancellationToken ct)
    {
        await userService.DeleteAsync(HttpContext, ct);

        return Ok(Results.Ok(HttpContext));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<Result>> Refresh(CancellationToken ct)
    {
        await tokenService.RefreshTokensAsync(HttpContext, ct);

        return Ok(Results.Ok(HttpContext));
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