using System.ComponentModel.DataAnnotations;

namespace AssetManagementBackend.DTOs;

public class UpdateUserDto
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(254)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string StaffId { get; set; } = string.Empty;
}