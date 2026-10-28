using System.ComponentModel.DataAnnotations;

namespace AssetManagementBackend.DTOs;

public class CreateBrandDto
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;
}