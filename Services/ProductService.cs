using AssetManagementBackend.Data;
using AssetManagementBackend.DTOs;
using AssetManagementBackend.Exceptions;
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
        if (page < 1)
        {
            throw new BadRequestException(
                "Page must be greater than zero.");
        }
        if (pageSize < 1 || pageSize > 100)
        {
            throw new BadRequestException(
                "Page size must be between 1 and 100");
        }

        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Brand)
            .Include(p => p.Images)
            .AsQueryable();

        if(!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();

            query = query.Where(product =>
            product.Name.Contains(searchTerm) ||
            product.Category.Name.Contains(searchTerm) ||
            product.Brand.Name.Contains(searchTerm));
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

    public async Task<ProductResponseDto> GetProduct(Guid id)
    {
        var product = await _context.Products
            .Include(product => product.Category)
            .Include(product => product.Brand)
            .Include(product => product.Images)
            .FirstOrDefaultAsync(product => product.Id == id);

        if (product == null)
        {
            throw new NotFoundException("Product was not found.");
        }

        return product.ToResponseDto();
    }

    public async Task<ProductResponseDto> CreateProduct(CreateProductDto dto)
    {
        if(dto.CategoryId == Guid.Empty)
        {
            throw new BadRequestException(
                "A valid category ID is required.");
        }

        if(dto.BrandId == Guid.Empty)
        {
            throw new BadRequestException(
                "A valid brand ID is required.");
        }

        var category = await _context.Categories
            .FindAsync(dto.CategoryId);

        if(category == null)
        {
            throw new NotFoundException(
                "The selected category was not found.");
        }

        var brand = await _context.Brands
            .FindAsync(dto.BrandId);

        if(brand == null)
        {
            throw new NotFoundException(
                "The selected brand was not found.");
        }

        var exists = await _context.Products
            .AnyAsync(product => product.Name == dto.Name.Trim());

        if (exists)
        {
            throw new ConflictException(
                "A product with this name already exists.");
        }

        var product = new Product
        {
            Name = dto.Name.Trim(),
            CategoryId = category.Id,
            Category = category,
            BrandId = brand.Id,
            Brand = brand,
            Description = dto.Description.Trim()
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        return product.ToResponseDto();
    }

    public async Task<ProductResponseDto> UpdateProduct(
        Guid id,
        UpdateProductDto dto)
    {
        var product = await _context.Products
            .Include(product => product.Category)
            .Include(product => product.Brand)
            .Include(product => product.Images)
            .FirstOrDefaultAsync(product => product.Id == id);

        if(product == null)
        {
            throw new NotFoundException("Product was not found.");
        }

        if(dto.CategoryId == Guid.Empty)
        {
            throw new BadRequestException(
                "A valid category ID is required.");
        }

        if(dto.BrandId == Guid.Empty)
        {
            throw new BadRequestException(
                "A valie brand ID is required.");
        }

        var category = await _context.Categories
            .FindAsync(dto.CategoryId);

        if(category == null)
        {
            throw new NotFoundException(
                "The selected category was not found.");
        }

        var brand = await _context.Brands
            .FindAsync(dto.BrandId);

        if(brand == null)
        {
            throw new NotFoundException(
                "The selected brand was not found.");
        }

        var duplicate = await _context.Products
            .AnyAsync(product => 
            product.Id != id &&
            product.Name == dto.Name.Trim());

        if (duplicate)
        {
            throw new ConflictException(
                "A product with this name already exists.");
        }

        product.Name = dto.Name.Trim();
        product.CategoryId = category.Id;
        product.Category = category;
        product.BrandId = brand.Id;
        product.Brand = brand;
        product.Description = dto.Description.Trim();

        await _context.SaveChangesAsync();

        return product.ToResponseDto();
    }

    public async Task DeleteProduct(Guid id)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            throw new NotFoundException("Product was not found.");
        }

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();
    }
}