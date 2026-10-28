using AssetManagementBackend.Models;

namespace AssetManagementBackend.DTOs;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string StaffId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role {  get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}