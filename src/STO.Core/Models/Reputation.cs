using System.ComponentModel.DataAnnotations;
using STO.Core.Enums;

namespace STO.Core.Models;

public class Reputation
{
    [Key]
    public int Id { get; set; }

    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    public ReputationFaction Faction { get; set; }

    /// <summary>Tier 0-6 (0 = not started, 6 = max)</summary>
    public int Tier { get; set; }

    public int Progress { get; set; }
}
