using System.Data;
using AssetManagementBackend.Data;
using AssetManagementBackend.DTOs;
using AssetManagementBackend.Exceptions;
using AssetManagementBackend.Mappings;
using AssetManagementBackend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AssetManagementBackend.Services;

public class OrderService
{
    private readonly ApplicationDbContext _context;

    public OrderService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResponseDto<OrderResponseDto>> GetOrders(
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

        var query = _context.Orders
            .AsNoTracking()
            .Include(o => o.Vendor)
            .Include(o => o.CreatedByUser)
            .Include(o => o.OrderItems)
            .ThenInclude(item => item.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();

            query = query.Where(o =>
            o.OrderNumber.Contains(searchTerm) ||
            o.Vendor.Name.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ThenByDescending(o => o.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var orderDtos = orders
            .Select(o => o.ToResponseDto())
            .ToList();

        return new PaginatedResponseDto<OrderResponseDto>
        {
            Data = orderDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<OrderResponseDto> GetOrder(Guid id)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Vendor)
            .Include(o => o.CreatedByUser)
            .Include(o => o.OrderItems)
            .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            throw new NotFoundException(
                "Order not found.");
        }

        return order.ToResponseDto();
    }

    public async Task<OrderResponseDto> CreateOrder(
        CreateOrderDto dto,
        Guid createdByUserId)
    {
        // Basic request validation
        if(dto.OrderItems is null || dto.OrderItems.Count == 0)
        {
            throw new BadRequestException(
                "An order must contain at least one item.");
        }

        if(dto.OrderItems.Any(item => item.ProductId == Guid.Empty))
        {
            throw new BadRequestException(
                "Every order item must have a valid product ID.");
        }

        if(dto.OrderItems.Any(item => item.Quantity <= 0))
        {
            throw new BadRequestException(
                "Every item quantity must be greater than zero.");
        }

        if(dto.OrderItems.Any(item => item.UnitPrice < 0))
        {
            throw new BadRequestException(
                "Unit price cannot be negative.");
        }

        if(dto.VendorId == Guid.Empty ||
            createdByUserId == Guid.Empty)
        {
            throw new BadRequestException(
                "A valid vendor and authenticated user are required.");
        }

        //Revent duplicate product lines in a single order.
        var productIds = dto.OrderItems
            .Select(item => item.ProductId)
            .ToList();

        if(productIds.Distinct().Count() != productIds.Count)
        {
            throw new BadRequestException(
                "Each product can only appear once in an order.");
        }

        // Use a serializable transaction to protect stock updates
        // from conflicting concurrent transactions.
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            var vendor = await _context.Vendors
                .SingleOrDefaultAsync(v => v.Id == dto.VendorId);

            if (vendor is null)
            {
                throw new NotFoundException("vendor not found.");
            }

            if (!vendor.IsActive)
            {
                throw new BadRequestException(
                    "Cannot create an order for an inactive vendor.");
            }

            var user = await _context.Users
                .SingleOrDefaultAsync(u => u.Id == createdByUserId);

            if(user is null || !user.IsActive)
            {
                throw new NotFoundException(
                    "The authenticated user was not found or is  inactive.");
            }

            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            var missingProductIds = productIds
                .Where(id => !products.ContainsKey(id))
                .ToList();

            if(missingProductIds.Count > 0)
            {
                throw new NotFoundException(
                    $"Product(s) not found.{string.Join(", ", missingProductIds)}");
            }

            // Get the next number from SQL Server's Sequence.
            var connection = _context.Database.GetDbConnection();


            await using var command = connection.CreateCommand();

            command.CommandText =
                 "SELECT NEXT VALUE FOR dbo.OrderNumberSequence";

            command.Transaction = _context.Database
                .CurrentTransaction?
                .GetDbTransaction();

            var result = await command.ExecuteScalarAsync();

            if(result is null || result == DBNull.Value)
            {
                throw new InvalidOperationException(
                    "Failed to generate the order number.");
            }

            var sequenceNumber = Convert.ToInt32(result);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = $"ORD{sequenceNumber:D4}",
                VendorId = vendor.Id,
                Vendor = vendor,
                OrderDate = dto.OrderDate,
                CreatedByUserId = user.Id,
                CreatedByUser = user,
                CreatedAt = DateTime.UtcNow
            };

            foreach(var itemDto in dto.OrderItems)
            {
                var product = products[itemDto.ProductId];

                var lineTotal =
                    itemDto.Quantity * itemDto.UnitPrice;

                var orderItem = new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    Order = order,
                    ProductId = product.Id,
                    Product = product,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice,
                    TotalPrice = lineTotal
                };

                order.OrderItems.Add(orderItem);
            }

            // Calculate totals on the server.
            order.TotalItems = order.OrderItems.Count;
            order.TotalQuantity = order.OrderItems.Sum(
                item => item.Quantity);
            order.TotalPrice = order.OrderItems.Sum(
                item => item.TotalPrice);

            // Record the order and all stock changes.
            _context.Orders.Add(order);

            foreach(var item in order.OrderItems)
            {
                var stock = await _context.Stocks
                    .SingleOrDefaultAsync(
                    s => s.ProductId == item.ProductId);

                if(stock is null)
                {
                    stock = new Stock
                    {
                        Id = Guid.NewGuid(),
                        ProductId = item.ProductId,
                        Product = item.Product,
                        QuantityAvailable = item.Quantity,
                        LowStockThreshold = 5,
                        LastUpdatedAt = DateTime.UtcNow
                    };

                    _context.Stocks.Add(stock);
                }

                else
                {
                    stock.QuantityAvailable += item.Quantity;
                    stock.LastUpdatedAt = DateTime.UtcNow;
                }

                var movement = new StockMovement
                {
                    Id = Guid.NewGuid(),
                    ProductId = item.ProductId,
                    Product = item.Product,
                    QuantityChange = item.Quantity,
                    MovementType = "Order Received",
                    OrderId = order.Id,
                    Order = order,
                    PerformedByUserId = user.Id,
                    PerformedByUser = user,
                    Notes = $"Stock received through {order.OrderNumber}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.StockMovements.Add(movement);
            }

            //Save everything together.
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return order.ToResponseDto();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}