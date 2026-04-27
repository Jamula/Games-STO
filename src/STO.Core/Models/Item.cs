using System.ComponentModel.DataAnnotations;
using STO.Core.Enums;

namespace STO.Core.Models;

/// <summary>
/// Reference item from the STO Wiki or manually added.
/// </summary>
public class Item
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public ItemType Type { get; set; }
    public ItemRarity? DefaultRarity { get; set; }

    [MaxLength(500)]
    public string? Source { get; set; }

    public string? Description { get; set; }

    [MaxLength(200)]
    public string? SetName { get; set; }

    [MaxLength(500)]
    public string? WikiUrl { get; set; }

    public bool IsFromWiki { get; set; }
    public DateTime? LastWikiSync { get; set; }

    public ICollection<InventoryItem> InventoryItems { get; set; } = [];
    public ICollection<BuildSlot> BuildSlots { get; set; } = [];
}
