using System.ComponentModel.DataAnnotations;
using STO.Core.Enums;

namespace STO.Core.Models;

/// <summary>
/// Tracks high-value items (T6 boxes, Promo boxes, Lobi items, etc.) across accounts.
/// </summary>
public class ValuableItem
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public ItemType ItemType { get; set; }

    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public int? CharacterId { get; set; }
    public Character? Character { get; set; }

    public InventoryLocation Location { get; set; }
    public int Quantity { get; set; } = 1;

    public string? Notes { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
