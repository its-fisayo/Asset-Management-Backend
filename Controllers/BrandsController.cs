using AssetManagementBackend.DTOs;
using AssetManagementBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetMAnagementBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BrandsController : ControllerBase
{
    private readonly BrandService _brandService;

    public BrandsController(BrandService brandService)
    {
        _brandService = brandService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBrands(string? search = null)
    {
        var brands = await _brandService.GetBrands(search);

        return Ok(brands);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBrand(Guid id)
    {
        var brand = await _brandService.GetBrand(id);

        return Ok(brand);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateBrand(CreateBrandDto dto)
    {
        var brand = await _brandService.CreateBrand(dto);

        return CreatedAtAction(
            nameof(GetBrand),
            new { id = brand.Id },
            brand);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBrand(
        Guid id,
        UpdateBrandDto dto)
    {
        var brand = await _brandService.UpdateBrand(id, dto);

        return Ok(brand);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBrand(Guid id)
    {
        await _brandService.DeleteBrand(id);

        return NoContent();
    }
}