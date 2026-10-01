using AssetManagementBackend.Data;
using AssetManagementBackend.DTOs;
using AssetManagementBackend.Mappings;
using AssetManagementBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetManagementBackend.Services;

public class ProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponseDto<ProductResponseDto>> GetProducts(
        int page,
        int pageSize,
        string? search)
    {
        var query = _context.Products.AsQueryable();

        if(!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(product =>
            product.Name.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(
            (double)totalCount / pageSize);
        var products = await query
            .OrderBy(product => product.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var productDtos = products
            .Select(product => product.ToResponseDto())
            .ToList();
        return new PaginatedResponseDto<ProductResponseDto>
        {
            Data = productDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<ProductResponseDto?> GetProduct(Guid id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return null;
        }

        return product.ToResponseDto();
    }

    public async Task<ProductResponseDto> CreateProduct(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Category = dto.Category,
            Brand = dto.Brand,
            Description = dto.Description
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return product.ToResponseDto();
    }

    public async Task<ProductResponseDto?> UpdateProduct(
        Guid id,
        UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);

        if(product == null)
        {
            return null;
        }

        product.Name = dto.Name;
        product.Category = dto.Category;
        product.Brand = dto.Brand;
        product.Description = dto.Description;

        await _context.SaveChangesAsync();

        return product.ToResponseDto();
    }

    public async Task<bool> DeleteProduct(Guid id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return false;
        }

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();

        return true;
    }
}