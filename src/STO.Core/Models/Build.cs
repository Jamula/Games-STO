using System.ComponentModel.DataAnnotations;
using STO.Core.Enums;

namespace STO.Core.Models;

public class Build
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public BuildType Type { get; set; }

    [MaxLength(200)]
    public string? ShipName { get; set; }

    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    public ICollection<BuildSlot> Slots { get; set; } = [];
}
