using AssetManagementBackend.Data;
using AssetManagementBackend.DTOs;
using AssetManagementBackend.Exceptions;
using AssetManagementBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetManagementBackend.Services;

public class CategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryResponseDto>> GetCategories(
        string? search)
    {
        var query = _context.Categories
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();
            query = query.Where(category =>
            category.Name.Contains(searchTerm));
        }

        return await query
            .OrderBy(category => category.Name)
            .Select(category => new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name
            })
            .ToListAsync();
    }

    public async Task<CategoryResponseDto> GetCategory(Guid id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(category => category.Id == id);

        if(category == null)
        {
            throw new NotFoundException("Category was not found.");
        }

        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name
        };
    }

    public async Task<CategoryResponseDto> CreateCategory(
        CreateCategoryDto dto)
    {
        var name = dto.Name.Trim();

        if(name.Length == 0)
        {
            throw new BadRequestException(
                "Category name cannot be empty.");
        }

        var exists = await _context.Categories
            .AnyAsync(category => category.Name == name);

        if(exists)
        {
            throw new ConflictException(
                "A category with this name already exists.");
        }

        var category = new Category
        {
            Name = name
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name
        };
    }

    public async Task<CategoryResponseDto> UpdateCategory(
        Guid id,
        UpdateCategoryDto dto)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(category =>category.Id == id);

        if(category == null)
        {
            throw new NotFoundException("Category was not found.");
        }

        var name = dto.Name.Trim();

        if(name.Length == 0)
        {
            throw new BadRequestException(
                "Category name cannot be empty.");
        }

        var duplicate = await _context.Categories
            .AnyAsync(existing =>
            existing.Id != id &&
            existing.Name == name);

        if (duplicate)
        {
            throw new ConflictException(
                "A category with this name already exists.");
        }

        category.Name = name;
        await _context.SaveChangesAsync();

        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name
        };
    }

    public async Task DeleteCategory(Guid id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(category => category.Id == id);

        if( category == null )
        {
            throw new NotFoundException("Category was not found.");
        }

        var inUse = await _context.Products
            .AnyAsync(product => product.CategoryId == id);

        if( inUse )
        {
            throw new ConflictException(
                "The category is assigned to one or more products " +
                "and cannot be deleted.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
    }
}