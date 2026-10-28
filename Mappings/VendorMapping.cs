using AssetManagementBackend.DTOs;
using AssetManagementBackend.Models;

namespace AssetManagementBackend.Mappings;

public static class VendorMapping
{
    public static VendorResponseDto ToResponseDto(
        this Vendor vendor)
    {
        return new VendorResponseDto
        {
            Id = vendor.Id,
            Name = vendor.Name,
            PrimaryContact = vendor.PrimaryContact,
            PhoneNumber = vendor.PhoneNumber,
            Email = vendor.Email,
            IsActive = vendor.IsActive
        };
    }
}