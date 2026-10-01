namespace AssetManagementBackend.Models;

public class ProductImage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePatch { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public Product Product { get; set; } = null;
}