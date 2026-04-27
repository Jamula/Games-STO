using System.ComponentModel.DataAnnotations;

namespace STO.Core.Models;

public class Account
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Gamertag { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Nickname { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Character> Characters { get; set; } = [];
    public ICollection<ValuableItem> ValuableItems { get; set; } = [];
}
