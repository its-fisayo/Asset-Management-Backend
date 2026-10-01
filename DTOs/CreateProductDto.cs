using System.ComponentModel.DataAnnotations;

namespace AssetManagementBackend.DTOs;

public class CreateProductDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = string.Empty;

    [Required]
    public string Brand {  get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}