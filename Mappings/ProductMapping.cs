using AssetManagementBackend.DTOs;
using AssetManagementBackend.Models;

namespace AssetManagementBackend.Mappings;

public static class ProductMapping
{
    public static ProductResponseDto ToResponseDto(
        this Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            BrandId = product.BrandId,
            BrandName = product.Brand?.Name ?? string.Empty,
            Description = product.Description,
            Images = product.Images
                .Select(image => new ProductImageResponseDto
                {
                    Id = image.Id,
                    ProductId = image.ProductId,
                    FileName = image.FileName,
                    FilePath = image.FilePath,
                    ContentType = image.ContentType,
                    FileSize = image.FileSize,
                    UploadedAt = image.UploadedAt
                })
                .ToList()
        };
    }
}