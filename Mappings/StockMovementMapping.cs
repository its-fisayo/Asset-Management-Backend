using AssetManagementBackend.DTOs;
using AssetManagementBackend.Models;

namespace AssetManagementBackend.Mappings;

public static class StockMovementMapping
{
    public static StockMovementResponseDto ToResponseDto(
        this StockMovement movement)
    {
        return new StockMovementResponseDto
        {
            Id = movement.Id,
            ProductId = movement.ProductId,
            ProductName = movement.Product.Name,
            QuantityChange = movement.QuantityChange,
            MovementType = movement.MovementType,
            OrderId = movement.OrderId,
            OrderNumber = movement.Order?.OrderNumber ?? string.Empty,
            IssuanceId = movement.IssuanceId,
            RecipientName = movement.Issuance?.RecipientName ?? string.Empty,
            PerformedByUserId = movement.PerformedByUserId,
            PerformedByUserName = movement.PerformedByUser.FullName,
            Notes = movement.Notes,
            CreatedAt = movement.CreatedAt
        };
    }
}