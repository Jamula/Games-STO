using System.ComponentModel.DataAnnotations;
using STO.Core.Enums;

namespace STO.Core.Models;

public class CharacterTrait
{
    [Key]
    public int Id { get; set; }

    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    public int TraitId { get; set; }
    public Trait Trait { get; set; } = null!;

    public TraitType SlotType { get; set; }
}
