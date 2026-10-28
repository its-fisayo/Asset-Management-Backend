using System.Data;
using AssetManagementBackend.Data;
using AssetManagementBackend.DTOs;
using AssetManagementBackend.Exceptions;
using AssetManagementBackend.Mappings;
using AssetManagementBackend.Models;
using Microsoft.EntityFrameworkCore;

namespace AssetManagementBackend.Services;

public class IssuanceService
{
    private readonly ApplicationDbContext _context;

    public IssuanceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IssuanceResponseDto> CreateIssuance(
        CreateIssuanceDto dto,
        Guid issuedByUserId)
    {
        // Validate the general request
        var recipientName = dto.RecipientName?.Trim();

        if (string.IsNullOrWhiteSpace(recipientName))
        {
            throw new BadRequestException(
                "Recipient name is required.");
        }

        if(recipientName.Length > 150)
        {
            throw new BadRequestException(
                "Recipient name cannot exceed 150 characters.");
        }

        if(dto.Items is null || dto.Items.Count == 0)
        {
            throw new BadRequestException(
                "An issuance must contain at least one item.");
        }

        if(issuedByUserId == Guid.Empty)
        {
            throw new BadRequestException(
                "A valid issuer is required.");
        }

        // Normalize and validate the item identifiers
        foreach (var item in dto.Items)
        {
            item.Barcode = item.Barcode?.Trim() ?? string.Empty;
            item.SerialNumber = item.SerialNumber?.Trim() ?? string.Empty;

            if (item.ProductId == Guid.Empty)
            {
                throw new BadRequestException(
                    "Every item must have a valid product ID.");
            }

            if (string.IsNullOrWhiteSpace(item.Barcode))
            {
                throw new BadRequestException(
                    "Every item must have a barcode.");
            }

            if (item.Barcode.Length > 100)
            {
                throw new BadRequestException(
                    "A barcode cannot exceed 100 characters.");
            }

            if (string.IsNullOrWhiteSpace(item.SerialNumber))
            {
                throw new BadRequestException(
                    "Every item must have a serial number.");
            }

            if (item.SerialNumber.Length > 100)
            {
                throw new BadRequestException(
                    "A serial number cannot exceed 100 characters.");
            }
        }

        // Each physical item needs unique identifiers
        var barcodes = dto.Items
            .Select(i => i.Barcode)
            .ToList();

        var serialNumbers = dto.Items
            .Select(i => i.SerialNumber)
            .ToList();

        if(barcodes.Distinct(
            StringComparer.OrdinalIgnoreCase).Count()
            != barcodes.Count)
        {
            throw new ConflictException(
                "Duuplicate barcodes were supplied.");
        }

        if (serialNumbers.Distinct(
            StringComparer.OrdinalIgnoreCase).Count()
            != serialNumbers.Count)
        {
            throw new ConflictException(
                "Duuplicate serial numbers were supplied.");
        }

        // Begin a transaction so stock and history remain consistent
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            // Confirm that the issuer exists and is active
            var issuer = await _context.Users
                .FirstOrDefaultAsync(u =>
                u.Id == issuedByUserId);

            if(issuer is null || !issuer.IsActive)
            {
                throw new BadRequestException(
                    "The issuing user does not exist or is inactive.");
            }

            // Check identifiers already recorded in the system
            var existingBarcode = await _context.IssuanceItems
                .AnyAsync(i => barcodes.Contains(i.Barcode));

            if (existingBarcode)
            {
                throw new ConflictException(
                    "One or more barcodes have already been used.");
            }

            var existingSerialNumber = await _context.IssuanceItems
                .AnyAsync(i => serialNumbers.Contains(i.SerialNumber));

            if (existingSerialNumber)
            {
                throw new ConflictException(
                    "One or more serial numbers have already been used.");
            }

            // Load all requested products
            var productIds = dto.Items
                .Select(i => i.ProductId)
                .Distinct()
                .ToList();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            if(products.Count != productIds.Count)
            {
                throw new NotFoundException(
                    "One or more requested products were not found.");
            }

            var productsById = products.ToDictionary(p => p.Id);

            // Load stock and check the total required per product
            var stocks = await _context.Stocks
                .Where(s => productIds.Contains(s.ProductId))
                .ToListAsync();

            var stocksByProductId = stocks
                .ToDictionary(s => s.ProductId);

            var requiredQuantities = dto.Items
                .GroupBy(i => i.ProductId)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var requirement in requiredQuantities)
            {
                if (!stocksByProductId.TryGetValue(
                    requirement.Key, out var stock))
                {
                    throw new BadRequestException(
                        $"No stock record exists for product " +
                        $"{productsById[requirement.Key].Name}.");
                }

                if (stock.QuantityAvailable < requirement.Value)
                {
                    throw new BadRequestException(
                        $"Insufficient stock for product " +
                        $"{productsById[requirement.Key].Name}. " +
                        $"Available: {stock.QuantityAvailable}; " +
                        $"requested: {requirement.Value}.");
                }
            }

            // Create the issuance and its individual item records
            var issuance = new Issuance
            {
                RecipientName = recipientName,
                IssuedByUserId = issuer.Id,
                IssuedByUser = issuer,
                IssuedAt = DateTime.UtcNow,
                Notes = string.IsNullOrWhiteSpace(dto.Notes)
                ? null : dto.Notes.Trim()
            };

            foreach (var itemDto in dto.Items)
            {
                issuance.Items.Add(new IssuanceItem
                {
                    ProductId = itemDto.ProductId,
                    Product = productsById[itemDto.ProductId],
                    Quantity = 1,
                    Barcode = itemDto.Barcode,
                    SerialNumber = itemDto.SerialNumber,
                });
            }

            _context.Issuances.Add(issuance);

            // Reduce stock and record one movement per physical item
            foreach(var item in issuance.Items)
            {
                var stock = stocksByProductId[item.ProductId];

                stock.QuantityAvailable -= 1;
                stock.LastUpdatedAt = DateTime.UtcNow;

                _context.StockMovements.Add(new StockMovement
                {
                    ProductId = item.ProductId,
                    QuantityChange = -1,
                    MovementType = "Issuance",
                    IssuanceId = issuance.Id,
                    PerformedByUserId = issuer.Id,
                    CreatedAt = DateTime.UtcNow,
                    Notes =
                    $"Barcode: {item.Barcode}; " +
                    $"Serial number: {item.SerialNumber}."
                });
            }

            // Save all changes, then committhe transaction
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Return the saved issuance details
            return issuance.ToResponseDto();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<PaginatedResponseDto<IssuanceResponseDto>> GetIssuances(
        int page = 1,
        int pageSize = 16,
        string? search = null)
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

        var query = _context.Issuances
            .AsNoTracking()
            .Include(i => i.IssuedByUser)
            .Include(i => i.Items)
            .ThenInclude(item => item.Product)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();

            query = query.Where(i =>
            i.RecipientName.Contains(searchTerm) ||
            i.IssuedByUser.FullName.Contains(searchTerm) ||
            i.Items.Any(item =>
                item.Barcode.Contains(searchTerm) ||
                item.SerialNumber.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var issuances = await query
            .OrderByDescending(i => i.IssuedAt)
            .ThenByDescending(i => i.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var issuanceDtos = issuances
            .Select(i => i.ToResponseDto())
            .ToList();

        return new PaginatedResponseDto<IssuanceResponseDto>
        {
            Data = issuanceDtos,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<IssuanceResponseDto> GetIssuance(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new BadRequestException(
                "A valid issuance ID is required.");
        }

        var issuance = await _context.Issuances
            .AsNoTracking()
            .Include(i => i.IssuedByUser)
            .Include(i => i.Items)
            .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(i => i.Id == id);

        if(issuance is null)
        {
            throw new NotFoundException(
                "Issuance not found.");
        }

        return issuance.ToResponseDto();
    }

    public async Task<IssuanceResponseDto> FindIssuanceByIdentifier(
        string? barcode,
        string? serialNumber)
    {
        barcode = string.IsNullOrWhiteSpace(barcode)
            ? null : barcode.Trim();

        serialNumber = string.IsNullOrWhiteSpace(serialNumber)
            ? null : serialNumber.Trim();

        // Require only one identifier
        if((barcode is null && serialNumber is null) ||
            (barcode is not null && serialNumber is not null))
        {
            throw new BadRequestException(
                "Provide either a barcode or a serial number, but not both.");
        }

        var itemQuery = _context.IssuanceItems
            .AsNoTracking()
            .AsQueryable();

        if(barcode is not null)
        {
            itemQuery = itemQuery.Where(
                item => item.Barcode == barcode);
        } else
        {
            itemQuery = itemQuery.Where(
                item => item.SerialNumber  == serialNumber);
        }

        var issuanceId = await itemQuery
            .Select(item => (Guid?)item.IssuanceId)
            .FirstOrDefaultAsync();

        if(issuanceId is null)
        {
            throw new NotFoundException(
                "No issuance was found for that identifier.");
        }

        return await GetIssuance(issuanceId.Value);
    }
}