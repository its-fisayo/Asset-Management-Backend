namespace AssetManagementBackend.DTOs;

public class ProductResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Brand {  get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}