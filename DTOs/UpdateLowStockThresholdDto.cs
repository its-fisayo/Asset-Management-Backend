using System.ComponentModel.DataAnnotations;

namespace AssetManagementBackend.DTOs;

public class UpdateLowStockThresholdDto
{
    [Range(5, int.MaxValue,
        ErrorMessage = "Threshold cannot be less than 5.")]
    public int LowStockThreshold { get; set; }
}