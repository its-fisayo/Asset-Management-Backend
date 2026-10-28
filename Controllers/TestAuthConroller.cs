using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagementBackend.Controllers;

[ApiController]
[Route("api/test-auth")]
public class TestAuthController : ControllerBase
{
    [HttpGet("public")]
    public IActionResult Public()
    {
        return Ok(new
        {
            message = "This endpoint is public."
        });
    }

    [Authorize]
    [HttpGet("protected")]
    public IActionResult Protected()
    {
        return Ok(new
        {
            message = "You are authenticated.",
            userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier),
            fullName = User.Identity?.Name,
            email = User.FindFirstValue(
                ClaimTypes.Email),
            staffId = User.FindFirstValue("StaffId"),
            role = User.FindFirstValue(ClaimTypes.Role)
        });
    }
}