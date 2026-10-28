namespace AssetManagementBackend.DTOs;

public class ProductResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid BrandId { get; set; }
    public string BrandName {  get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ProductImageResponseDto> Images { get; set; } = new();
}