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
            Category = product.Category,
            Brand = product.Brand,
            Description = product.Description
        };
    }
}