namespace AssetManagementBackend.Models;

public class StockMovement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int QuantityChange { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public Order? Order { get; set; }
    public Guid? IssuanceId { get; set; }
    public Issuance? Issuance { get; set; }
    public Guid PerformedByUserId { get; set; }
    public User PerformedByUser { get; set; } = null!;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}