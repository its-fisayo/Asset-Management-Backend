namespace AssetManagementBackend.Models;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OrderNumber { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;
    public DateTime OrderDate { get; set; }
    public int TotalQuantity { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalPrice { get; set; }
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<OrderItem> OrderItems { get; set; }
    = new List<OrderItem>();
}