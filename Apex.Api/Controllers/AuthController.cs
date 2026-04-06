using Apex.Application.DTOs;
using Apex.Application.Interfaces;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Apex.Api.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService _authService = authService;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);

        if (response == null)
        {
            // Security Tip: Don't tell them if the user exists or password is wrong
            // Just say "Unauthorized" to prevent user enumeration attacks.
            return Unauthorized(new ErrorResponse(401, "Invalid username or password."));
        }

        return Ok(response);
    }
}