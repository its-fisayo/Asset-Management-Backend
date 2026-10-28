using AssetManagementBackend.DTOs;
using AssetManagementBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagementBackend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts(
        int page = 1,
        int pageSize = 16,
        string? search = null)
    {
        var products = await _productService.GetProducts(
            page,
            pageSize,
            search);

        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var product = await _productService.GetProduct(id);

        return Ok(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductDto dto)
    {
        var product = await _productService.CreateProduct(dto);

        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            product
        );
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        UpdateProductDto dto
        )
    {
        var product = await _productService.UpdateProduct(id, dto);

        return Ok(product);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        await _productService.DeleteProduct(id);

        return NoContent();
    }
}