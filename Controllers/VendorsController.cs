using AssetManagementBackend.DTOs;
using AssetManagementBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagementBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class VendorsController : ControllerBase
{
    private readonly VendorService _vendorService;

    public VendorsController(VendorService vendorService)
    {
        _vendorService = vendorService;
    }

    [HttpGet]
    public async Task<IActionResult> GetVendors(
        int page = 1,
        int pageSize = 16,
        string? search = null)
    {
        var vendors = await _vendorService.GetVendors(
            page,
            pageSize,
            search);

        return Ok(vendors);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetVendor(Guid id)
    {
        var vendor = await _vendorService.GetVendor(id);

        return Ok(vendor);
    }


    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateVendor(CreateVendorDto dto)
    {
        var vendor = await _vendorService.CreateVendor(dto);

        return CreatedAtAction(
            nameof(GetVendor),
            new { id = vendor.Id },
            vendor
        );
    }


    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVendor(
        Guid id,
        UpdateVendorDto dto)
    {
        var vendor = await _vendorService.UpdateVendor(id, dto);

        return Ok(vendor);
    }


    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVendor(Guid id)
    {
        await _vendorService.DeleteVendor(id);

        return NoContent();
    }
}