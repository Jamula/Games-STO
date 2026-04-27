using Microsoft.EntityFrameworkCore;
using STO.Core.Models;
using STO.Core.Services;
using STO.Data.Context;

namespace STO.Data.Services;

public class ReputationService(StoDbContext db) : IReputationService
{
    public async Task<IReadOnlyList<Reputation>> GetByCharacterIdAsync(int characterId)
    {
        return await db.Reputations
            .Where(r => r.CharacterId == characterId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Reputation> UpdateAsync(Reputation reputation)
    {
        db.Reputations.Update(reputation);
        await db.SaveChangesAsync();
        return reputation;
    }

    public async Task<IReadOnlyList<Reputation>> GetAllForAccountAsync(int accountId)
    {
        return await db.Reputations
            .Include(r => r.Character)
            .Where(r => r.Character.AccountId == accountId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Reputation>> GetReputationSummaryAsync(int characterId)
    {
        return await db.Reputations
            .Where(r => r.CharacterId == characterId)
            .OrderBy(r => r.Faction)
            .AsNoTracking()
            .ToListAsync();
    }
}
