using AssetManagementBackend.DTOs;
using AssetManagementBackend.Models;

namespace AssetManagementBackend.Mappings;

public static class IssuanceMapping
{
    public static IssuanceResponseDto ToResponseDto(
        this Issuance issuance)
    {
        return new IssuanceResponseDto
        {
            Id = issuance.Id,
            RecipientName = issuance.RecipientName,
            IssuedByUserId = issuance.IssuedByUserId,
            IssuedByUserName = issuance.IssuedByUser.FullName,
            IssuedAt = issuance.IssuedAt,
            Notes = issuance.Notes,
            TotalItems = issuance.Items.Count,
            Items = issuance.Items
            .Select(item => new IssuanceItemResponseDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                Barcode = item.Barcode,
                SerialNumber = item.SerialNumber
            }).ToList()
        };
    }
}