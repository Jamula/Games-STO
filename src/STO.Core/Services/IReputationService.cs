using STO.Core.Models;

namespace STO.Core.Services;

public interface IReputationService
{
    Task<IReadOnlyList<Reputation>> GetByCharacterIdAsync(int characterId);
    Task<Reputation> UpdateAsync(Reputation reputation);
    Task<IReadOnlyList<Reputation>> GetAllForAccountAsync(int accountId);
    Task<IReadOnlyList<Reputation>> GetReputationSummaryAsync(int characterId);
}
