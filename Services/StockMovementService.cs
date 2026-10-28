using AssetManagementBackend.Data;
using AssetManagementBackend.DTOs;
using AssetManagementBackend.Exceptions;
using AssetManagementBackend.Mappings;
using Microsoft.EntityFrameworkCore;

namespace AssetManagementBackend.Services;

public class StockMovementService
{
    private readonly ApplicationDbContext _context;

    public StockMovementService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponseDto<StockMovementResponseDto>> GetStockMovements(
        int page = 1,
        int pageSize = 16,
        string? search = null,
        Guid? productId = null,
        string? movementType = null)
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

        // Start with all stock movements
        var query = _context.StockMovements
            .AsNoTracking()
            .Include(m => m.Product)
            .Include(m => m.PerformedByUser)
            .Include(m => m.Order)
            .Include(m => m.Issuance)
            .AsQueryable();

        // Filter by product
        if (productId.HasValue)
        {
            query = query.Where(
                m => m.ProductId == productId.Value);
        }

        // Filter by movement type
        if (!string.IsNullOrWhiteSpace(movementType))
        {
            var movementTypeTerm = movementType.Trim();

            query = query.Where(
                m => m.MovementType.Contains(movementTypeTerm));
        }

        // Search by product name, order number,
        // recipient name and notes.
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();

            query = query.Where(m => 
            m.Product.Name.Contains(searchTerm) ||
            (m.Order != null &&
            m.Order.OrderNumber.Contains(searchTerm)) ||
            (m.Issuance != null &&
            m.Issuance.RecipientName.Contains(searchTerm)) ||
            (m.Notes != null &&
            m.Notes.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var movements = await query
            .OrderByDescending(m => m.CreatedAt)
            .ThenByDescending(m => m.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var movementDtos = movements
            .Select(m => m.ToResponseDto())
            .ToList();

        return new PaginatedResponseDto<StockMovementResponseDto>
        {
            Data = movementDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<StockMovementResponseDto> GetStockMovement(Guid id)
    {
        var movement = await _context.StockMovements
            .AsNoTracking()
            .Include(m => m.Product)
            .Include(m => m.PerformedByUser)
            .Include(m => m.Order)
            .Include(m => m.Issuance)
            .FirstOrDefaultAsync(m => m.Id == id);

        if(movement == null)
        {
            throw new NotFoundException(
                "Stock movement not found.");
        }

        return movement.ToResponseDto();
    }
}