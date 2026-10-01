using AssetManagementBackend.DTOs;
using AssetManagementBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace AssetManagementBackend.Controllers;

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
        if (page < 1)
        {
            page = 1;
        }
        if (pageSize < 1)
        {
            pageSize = 16;
        }
        if (pageSize > 100)
        {
            pageSize = 100;
        }
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

        if(product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

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

    [HttpPut]
    public async Task<IActionResult> UpdateProduct(
        Guid id,
        UpdateProductDto dto
        )
    {
        var product = await _productService.UpdateProduct(id, dto);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var deleted = await _productService.DeleteProduct(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}