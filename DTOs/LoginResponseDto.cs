namespace AssetManagementBackend.DTOs;

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    public DateTime ExpiresAt { get; set; }

    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string StaffId { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}