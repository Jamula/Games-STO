using System.ComponentModel.DataAnnotations;
using STO.Core.Enums;

namespace STO.Core.Models;

public class InventoryItem
{
    [Key]
    public int Id { get; set; }

    public int ItemId { get; set; }
    public Item Item { get; set; } = null!;

    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    public int Quantity { get; set; } = 1;
    public InventoryLocation Location { get; set; }
    public ItemRarity? Rarity { get; set; }

    public string? Notes { get; set; }
}
