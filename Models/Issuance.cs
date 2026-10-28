namespace AssetManagementBackend.Models;

public class Issuance
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // The receiver
    public string RecipientName { get; set; } = string.Empty;

    // The issuer
    public Guid IssuedByUserId { get; set; }
    public User IssuedByUser { get; set; } = null!;

    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    public ICollection<IssuanceItem> Items { get; set; }
    = new List<IssuanceItem>();
}