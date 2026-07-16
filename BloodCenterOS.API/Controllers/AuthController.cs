using BloodCenterOS.Core.Models;
using BloodCenterOS.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            if (result is null)
                return Unauthorized(ApiResponse<LoginResponse>.Fail("Invalid credentials"));

            return Ok(ApiResponse<LoginResponse>.Ok(result));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<LoginResponse>.Fail($"An unexpected error occurred: {ex.Message}"));
        }
    }
}
