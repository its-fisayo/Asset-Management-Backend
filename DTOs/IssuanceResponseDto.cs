namespace AssetManagementBackend.DTOs;

public class IssuanceResponseDto
{
    public Guid Id { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public Guid IssuedByUserId { get; set; }
    public string IssuedByUserName { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public string? Notes {  get; set; }
    public int TotalItems { get; set; }
    public List<IssuanceItemResponseDto> Items { get; set; } = new();
}

public class IssuanceItemResponseDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string SerialNumber {  get; set; } = string.Empty;
}