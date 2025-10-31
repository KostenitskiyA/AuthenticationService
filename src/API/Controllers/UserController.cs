using API.Models;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Results = Microsoft.AspNetCore.Http.Results;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService) : ControllerBase
{
    [Authorize]
    [HttpDelete("delete")]
    public async Task<ActionResult<Result>> DeleteAsync(CancellationToken ct)
    {
        await userService.DeleteAsync(HttpContext, ct);

        return Ok(Results.Ok(HttpContext));
    }
}
