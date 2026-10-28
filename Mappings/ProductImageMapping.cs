using AssetManagementBackend.DTOs;
using AssetManagementBackend.Models;

namespace AssetManagementBackend.Mappings;

public static class ProductImageMapping
{
    public static ProductImageResponseDto ToResponseDto(
        this ProductImage image)
    {
        return new ProductImageResponseDto
        {
            Id = image.Id,
            ProductId = image.ProductId,
            FileName = image.FileName,
            FilePath = image.FilePath,
            ContentType = image.ContentType,
            FileSize = image.FileSize,
            UploadedAt = image.UploadedAt
        };
    }
}