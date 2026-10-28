using System.ComponentModel.DataAnnotations;

namespace AssetManagementBackend.DTOs;

public class CreateProductDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    public Guid BrandId { get; set; }

    public string Description { get; set; } = string.Empty;
}