namespace AssetManagementBackend.Models;

public class Vendor
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string PrimaryContact { get; set; } = string.Empty;
    public string PhoneNumber {  get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}