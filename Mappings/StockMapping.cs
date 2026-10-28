using AssetManagementBackend.DTOs;
using AssetManagementBackend.Models;

namespace AssetManagementBackend.Mappings;

public static class StockMapping
{
    public static StockResponseDto ToResponseDto(
        this Stock stock)
    {
        return new StockResponseDto
        {
            Id = stock.Id,
            ProductId = stock.ProductId,
            ProductName = stock.Product.Name,
            Category = stock.Product.Category?.Name ?? string.Empty,
            Brand = stock.Product.Brand?.Name ?? string.Empty,
            QuantityAvailable = stock.QuantityAvailable,
            LowStockThreshold = stock.LowStockThreshold,
            LastUpdated = stock.LastUpdatedAt
        };
    }
}