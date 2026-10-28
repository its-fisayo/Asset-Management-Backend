using AssetManagementBackend.DTOs;
using AssetManagementBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetMAnagementBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoriesController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories(string? search = null)
    {
        var categories = await _categoryService.GetCategories(search);

        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategory(Guid id)
    {
        var category = await _categoryService.GetCategory(id);

        return Ok(category);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryDto dto)
    {
        var category = await _categoryService.CreateCategory(dto);

        return CreatedAtAction(
            nameof(GetCategory),
            new { id = category.Id },
            category);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        UpdateCategoryDto dto)
    {
        var category = await _categoryService.UpdateCategory(id, dto);

        return Ok(category);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        await _categoryService.DeleteCategory(id);

        return NoContent();
    }
}