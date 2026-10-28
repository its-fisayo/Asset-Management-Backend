using AssetManagementBackend.DTOs;
using AssetManagementBackend.Models;

namespace AssetManagementBackend.Mappings;

public static class OrderMapping
{
    public static OrderResponseDto ToResponseDto(
        this Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            VendorId = order.VendorId,
            VendorName = order.Vendor.Name,
            OrderDate = order.OrderDate,
            TotalItems = order.TotalItems,
            TotalQuantity = order.TotalQuantity,
            TotalPrice = order.TotalPrice,
            CreatedByUserId = order.CreatedByUserId,
            CreatedByUserName = order.CreatedByUser.FullName,
            CreatedAt = order.CreatedAt,
            OrderItems = order.OrderItems.Select(item =>
            new OrderItemResponseDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice
            }).ToList()
        };
    }
}