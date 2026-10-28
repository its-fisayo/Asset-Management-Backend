using System.ComponentModel.DataAnnotations;

namespace AssetManagementBackend.DTOs;

public class CreateOrderDto
{
    [Required]
    public Guid VendorId { get; set; }
    [Required]
    public DateTime OrderDate { get; set; }
    [Required]
    [MinLength(1, ErrorMessage = "An order must contain at least one item.")]
    public List<CreateOrderItemDto> OrderItems { get; set; } = new();
}

public class CreateOrderItemDto
{
    [Required]
    public Guid ProductId { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
    [Range(typeof(decimal), "0", "9999999999999999.99",
        ErrorMessage = "Unit price cannot be negative.")]
    public decimal UnitPrice { get; set; }
}