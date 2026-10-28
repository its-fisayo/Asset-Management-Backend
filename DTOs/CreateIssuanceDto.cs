using System.ComponentModel.DataAnnotations;

namespace AssetManagementBackend.DTOs;

public class CreateIssuanceDto
{
	[Required]
	[StringLength(150, MinimumLength = 2)]
	public string RecipientName { get; set; } = string.Empty;

	[StringLength(500)]
	public string? Notes {  get; set; }

	[Required]
	[MinLength(1, ErrorMessage = "An issuance must contain at least one item.")]
	public List<CreateIssuanceItemDto> Items { get; set; } = new();
}

public class CreateIssuanceItemDto
{
	[Required]
	public Guid ProductId { get; set; }

	[Required]
	[StringLength(100)]
	public string Barcode { get; set; } = string.Empty;

	[Required]
	[StringLength(100)]
	public string SerialNumber { get; set; } = string.Empty;
}