using API.Models;
using Application.Dtos;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController(IUserService userService, ITokenService tokenService) : ControllerBase
{
    [HttpPost("signup")]
    public async Task<ActionResult<Result>> SignUp([FromBody] SignInRequest request, CancellationToken ct)
    {
        await userService.SignUpAsync(request, ct);
        return NoContent();
    }

    [HttpPost("login")]
    public async Task<ActionResult<Result>> LogIn([FromBody] LogInRequest request, CancellationToken ct)
    {
        await userService.LogInAsync(request, ct);
        return NoContent();
    }

    [Authorize]
    [HttpDelete("logout")]
    public async Task<ActionResult<Result>> LogOut()
    {
        await tokenService.RevokeTokensAsync();
        return NoContent();
    }


    [HttpPost("refresh")]
    public async Task<ActionResult<Result>> Refresh(CancellationToken ct)
    {
        await tokenService.RefreshTokensAsync(ct);
        return NoContent();
    }
}
