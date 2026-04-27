using System.ComponentModel.DataAnnotations;

namespace STO.Core.Models;

public class BuildSlot
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string SlotName { get; set; } = string.Empty;

    public int Position { get; set; }

    public int BuildId { get; set; }
    public Build Build { get; set; } = null!;

    public int? ItemId { get; set; }
    public Item? Item { get; set; }

    public string? Notes { get; set; }
}
