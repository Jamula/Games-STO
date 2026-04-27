using System.ComponentModel.DataAnnotations;

namespace STO.Core.Models;

public class AdmiraltyShip
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public int Engineering { get; set; }
    public int Science { get; set; }
    public int Tactical { get; set; }

    [MaxLength(500)]
    public string? SpecialAbility { get; set; }

    public bool IsOneTimeUse { get; set; }

    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;
}
