namespace AssetManagementBackend.DTOs;

public class StockMovementResponseDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int QuantityChange { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid? IssuanceId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public Guid PerformedByUserId { get; set; }
    public string PerformedByUserName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}