using System.Security.Claims;
using AssetManagementBackend.DTOs;
using AssetManagementBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetMAnagementBAckend.Controller;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    // Get all users
    [HttpGet]
    public async Task<IActionResult> GetUSers(
        int page = 1,
        int pageSize = 16,
        string? search = null)
    {
        var users = await _userService.GetUsers(
            page, pageSize, search);

        return Ok(users);
    }

    // Get user by id
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUSer(Guid id)
    {
        var user = await _userService.GetUser(id);

        return Ok(user);
    }

    // Update user details 
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        UpdateUserDto dto)
    {
        var user = await _userService.UpdateUser(id, dto);

        return Ok(user);
    }

    // Update user status
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateUserStatus(
        Guid id,
        UpdateUserStatusDto dto)
    {
        if(IsCurrentUser(id) && !dto.IsActive)
        {
            return BadRequest(new
            {
                message = "You cannot deactivate your own account."
            });
        }

        var user = await _userService.UpdateUserStatus(
            id,
            dto.IsActive);

        return Ok(user);
    }

    // Update user role
    [HttpPatch("{id}/role")]
    public async Task<IActionResult> UpdateUserRole(
        Guid id,
        UpdateUserRoleDto dto)
    {
        if (IsCurrentUser(id))
        {
            return BadRequest(new
            {
                message = "You cannot change your own role."
            });
        }

        var user = await _userService.UpdateUserRole(
            id,
            dto.Role);

        return Ok(user);
    }

    // Compare the route Id with the authenticated user's ID claim.
    private bool IsCurrentUser(Guid id)
    {
        var currentUserId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return Guid.TryParse(currentUserId, out var parsedId)
            && parsedId == id;
    }
}