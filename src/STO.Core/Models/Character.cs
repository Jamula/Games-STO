using System.ComponentModel.DataAnnotations;
using STO.Core.Enums;

namespace STO.Core.Models;

public class Character
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public Career Career { get; set; }
    public Faction Faction { get; set; }
    public int Level { get; set; } = 65;

    [MaxLength(100)]
    public string? ActiveShip { get; set; }

    public string? Notes { get; set; }

    public int AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public ICollection<Build> Builds { get; set; } = [];
    public ICollection<InventoryItem> InventoryItems { get; set; } = [];
    public ICollection<Reputation> Reputations { get; set; } = [];
    public ICollection<CharacterTrait> CharacterTraits { get; set; } = [];
    public ICollection<AdmiraltyShip> AdmiraltyShips { get; set; } = [];
    public ICollection<DoffAssignment> DoffAssignments { get; set; } = [];
}
