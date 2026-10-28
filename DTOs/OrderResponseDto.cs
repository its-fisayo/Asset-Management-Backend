namespace AssetManagementBackend.DTOs;

public class OrderResponseDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public int TotalItems { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalPrice { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<OrderItemResponseDto> OrderItems { get; set; } = new();
}

public class OrderItemResponseDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}