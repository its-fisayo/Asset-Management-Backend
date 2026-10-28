using System.Security.Claims;
using AssetManagementBackend.DTOs;
using AssetManagementBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagementBackend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class IssuancesController : ControllerBase
{
    private readonly IssuanceService _issuanceService;

    public IssuancesController(IssuanceService issuanceService)
    {
        _issuanceService = issuanceService;
    }

    [HttpGet]
    public async Task<IActionResult> GetIssuances(
        int page = 1,
        int pageSize = 16,
        string? search = null)
    {
        var issuances = await _issuanceService.GetIssuances(
            page,
            pageSize,
            search);

        return Ok(issuances);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetIssuance(Guid id)
    {
        var issuance = await _issuanceService.GetIssuance(id);

        return Ok(issuance);
    }

    [HttpGet("items/lookup")]
    public async Task<IActionResult> FindIssuanceByIdentifier(
        string? barcode = null,
        string? serialNumber = null)
    {
        var issuance = await _issuanceService.FindIssuanceByIdentifier(
            barcode, serialNumber);

        return Ok(issuance);
    }

    [HttpPost]
    public async Task<IActionResult> CreateIssuance(
        CreateIssuanceDto dto)
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if(!Guid.TryParse(userIdClaim, out var issuedByUserId))
        {
            return Unauthorized(new
            {
                message = "The authenticated user's ID is invalid."
            }); 
        }

        var issuance = await _issuanceService.CreateIssuance(
            dto,
            issuedByUserId);

        return CreatedAtAction(
            nameof(GetIssuance),
            new { id = issuance.Id },
            issuance);
    }
}