namespace AssetManagementBackend.Models;

public class Stock
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int QuantityAvailable { get; set; }
    public int LowStockThreshold { get; set; } = 5;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}