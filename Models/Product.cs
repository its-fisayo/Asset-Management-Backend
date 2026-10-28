namespace AssetManagementBackend.Models;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public Guid BrandId { get; set; }
    public Brand Brand {  get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public ICollection<ProductImage> Images { get; set; }
    = new List<ProductImage>();
}