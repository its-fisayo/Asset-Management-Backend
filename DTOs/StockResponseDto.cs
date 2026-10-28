namespace AssetManagementBackend.DTOs;

public class StockResponseDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int QuantityAvailable { get; set; }
    public int LowStockThreshold { get; set; }
    public bool IsLowStock =>
        QuantityAvailable <= LowStockThreshold;
    public DateTime LastUpdated { get; set; }
}