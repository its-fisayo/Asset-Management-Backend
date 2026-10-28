using AssetManagementBackend.DTOs;
using AssetManagementBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagementBackend.Controllers;

[Authorize] // All endpoints require an authenticated user by default.
[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
	private readonly ProductImageService _imageService;

	public FilesController(ProductImageService imageService)
	{
		_imageService = imageService;
	}

	// Only admin users can upload images.
	[Authorize(Roles = "Admin")]
	[HttpPost("upload")]
	public async Task<IActionResult> Upload(
		Guid productId,
		IFormFile file)
	{
		var image = await _imageService.UploadImage(
			productId,
			file);

		if (image == null)
		{
			return NotFound("Product was not found.");
		}

		return Ok(image);
	}

	// Any authenticated user can retrieve image metadata for a product.
	[HttpGet("product/{productId}")]
	public async Task<IActionResult> GetProductImages(Guid productId)
	{
		var images = await _imageService.GetProductImages(productId);

		return Ok(images);
	}

	// Only admin users can delet images.
	[Authorize(Roles = "Admin")]
	[HttpDelete("{imageId}")]
	public async Task<IActionResult> DeleteImage(Guid imageId)
	{
		var deleted = await _imageService.DeleteImage(imageId);

		if (!deleted)
		{
			return NotFound("Image was not found.");
		}

		return NoContent();
	}
}