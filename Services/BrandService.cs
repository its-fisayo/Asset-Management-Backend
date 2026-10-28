using AssetManagementBackend.Data;
using AssetManagementBackend.DTOs;
using AssetManagementBackend.Exceptions;
using AssetManagementBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetManagementBackend.Services;

public class BrandService
{
    private readonly ApplicationDbContext _context;

    public BrandService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<BrandResponseDto>> GetBrands(
        string? search)
    {
        var query = _context.Brands
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();
            query = query.Where(brand =>
            brand.Name.Contains(searchTerm));
        }

        return await query
            .OrderBy(brand => brand.Name)
            .Select(brand => new BrandResponseDto
            {
                Id = brand.Id,
                Name = brand.Name
            })
            .ToListAsync();
    }

    public async Task<BrandResponseDto> GetBrand(Guid id)
    {
        var brand = await _context.Brands
            .AsNoTracking()
            .FirstOrDefaultAsync(brand => brand.Id == id);

        if (brand == null)
        {
            throw new NotFoundException("Brand was not found.");
        }

        return new BrandResponseDto
        {
            Id = brand.Id,
            Name = brand.Name
        };
    }

    public async Task<BrandResponseDto> CreateBrand(
        CreateBrandDto dto)
    {
        var name = dto.Name.Trim();

        if (name.Length == 0)
        {
            throw new BadRequestException(
                "Brand name cannot be empty.");
        }

        var exists = await _context.Brands
            .AnyAsync(brand => brand.Name == name);

        if (exists)
        {
            throw new ConflictException(
                "A brand with this name already exists.");
        }

        var brand = new Brand
        {
            Name = name
        };

        _context.Brands.Add(brand);
        await _context.SaveChangesAsync();

        return new BrandResponseDto
        {
            Id = brand.Id,
            Name = brand.Name
        };
    }

    public async Task<BrandResponseDto> UpdateBrand(
        Guid id,
        UpdateBrandDto dto)
    {
        var brand = await _context.Brands
            .FirstOrDefaultAsync(brand => brand.Id == id);

        if (brand == null)
        {
            throw new NotFoundException("Brand was not found.");
        }

        var name = dto.Name.Trim();

        if (name.Length == 0)
        {
            throw new BadRequestException(
                "Brand name cannot be empty.");
        }

        var duplicate = await _context.Brands
            .AnyAsync(existing =>
            existing.Id != id &&
            existing.Name == name);

        if (duplicate)
        {
            throw new ConflictException(
                "A brand with this name already exists.");
        }

        brand.Name = name;
        await _context.SaveChangesAsync();

        return new BrandResponseDto
        {
            Id = brand.Id,
            Name = brand.Name
        };
    }

    public async Task DeleteBrand(Guid id)
    {
        var brand = await _context.Brands
            .FirstOrDefaultAsync(brand => brand.Id == id);

        if (brand == null)
        {
            throw new NotFoundException("Brand was not found.");
        }

        var inUse = await _context.Products
            .AnyAsync(product => product.BrandId == id);

        if (inUse)
        {
            throw new ConflictException(
                "The brand is assigned to one or more products " +
                "and cannot be deleted.");
        }

        _context.Brands.Remove(brand);
        await _context.SaveChangesAsync();
    }
}