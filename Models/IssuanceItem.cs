namespace AssetManagementBackend.Models;

public class IssuanceItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid IssuanceId { get; set; }
    public Issuance Issuance { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // Each record represents one physical item
    public int Quantity { get; set; } = 1;

    public string Barcode { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
}