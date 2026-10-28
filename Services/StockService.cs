using AssetManagementBackend.Data;
using AssetManagementBackend.DTOs;
using AssetManagementBackend.Mappings;
using AssetManagementBackend.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AssetManagementBackend.Services;

public class StockService
{
    private readonly ApplicationDbContext _context;

    public StockService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponseDto<StockResponseDto>> GetStocks(
        int page = 1,
        int pageSize = 16,
        string? search = null,
        bool lowStockOnly = false)
    {
        if(page < 1)
        {
            throw new BadRequestException(
                "Page must be greater than zero.");
        }

        if(pageSize < 1 || pageSize > 100)
        {
            throw new BadRequestException(
                "Page size must be between 1 and 100.");
        }

        var query = _context.Stocks
            .AsNoTracking()
            .Include(s => s.Product)
            .ThenInclude(p => p.Category)
            .Include(s => s.Product)
            .ThenInclude(p => p.Brand)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();

            query = query.Where(s =>
            s.Product.Name.Contains(searchTerm) ||
            s.Product.Category.Name.Contains(searchTerm) ||
            s.Product.Brand.Name.Contains(searchTerm));
        }

        if (lowStockOnly)
        {
            query = query.Where(s =>
            s.QuantityAvailable <= s.LowStockThreshold);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var stocks = await query
            .OrderBy(s => s.Product.Name)
            .ThenBy(s => s.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var stockDtos = stocks
            .Select(s => s.ToResponseDto())
            .ToList();

        return new PaginatedResponseDto<StockResponseDto>
        {
            Data = stockDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<StockResponseDto> GetStockByProductId(
        Guid productId)
    {
        if(productId == Guid.Empty)
        {
            throw new BadRequestException(
                "A valid product ID is required.");
        }

        var stock = await _context.Stocks
            .AsNoTracking()
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s =>
            s.ProductId == productId);

        if(stock is null)
        {
            throw new BadRequestException(
                "Stock record not found for this product.");
        }

        return stock.ToResponseDto();
    }

    public async Task<StockResponseDto> UpdateLowStockThreshold(
        Guid productId,
        int lowStockThreshold)
    {
        if(productId == Guid.Empty)
        {
            throw new BadRequestException(
                "A valid product ID is required.");
        }

        if(lowStockThreshold < 5)
        {
            throw new BadRequestException(
                "Low-stock threshold cannot be less than 5");
        }

        var stock = await _context.Stocks
            .Include(s => s.Product)
            .FirstOrDefaultAsync(s => s.ProductId == productId);

        if(stock == null)
        {
            throw new NotFoundException(
                "Stock record not found for this product.");
        }

        stock.LowStockThreshold = lowStockThreshold;
        stock.LastUpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return stock.ToResponseDto();
    }
}