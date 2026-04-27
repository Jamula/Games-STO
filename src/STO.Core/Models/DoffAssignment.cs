using System.ComponentModel.DataAnnotations;

namespace STO.Core.Models;

public class DoffAssignment
{
    [Key]
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Type { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "Available";

    public DateTime? CompletesAt { get; set; }

    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;
}
