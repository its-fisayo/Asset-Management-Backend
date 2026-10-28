using AssetManagementBackend.DTOs;
using AssetManagementBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagementBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("signup")]
    public async Task<IActionResult> Signup(
        SignupRequestDto dto)
    {
        var result = await _authService.Signup(dto);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequestDto dto)
    {
        var result = await _authService.Login(dto);

        if(result == null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        return Ok(result);
    }
}