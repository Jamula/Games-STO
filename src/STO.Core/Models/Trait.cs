using System.ComponentModel.DataAnnotations;
using STO.Core.Enums;

namespace STO.Core.Models;

public class Trait
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public TraitType Type { get; set; }

    [MaxLength(500)]
    public string? Source { get; set; }

    public string? Description { get; set; }

    [MaxLength(500)]
    public string? WikiUrl { get; set; }

    public bool IsFromWiki { get; set; }

    public ICollection<CharacterTrait> CharacterTraits { get; set; } = [];
}
