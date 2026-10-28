using System.ComponentModel.DataAnnotations;

namespace AssetManagementBackend.DTOs;

public class UpdateVendorDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string PrimaryContact { get; set; } = string.Empty;

    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}