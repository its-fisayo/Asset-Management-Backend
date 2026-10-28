namespace AssetManagementBackend.DTOs;

public class ProductImageResponseDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string ContentType {  get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime UploadedAt { get; set; }
}