using Microsoft.EntityFrameworkCore;
using STO.Core.Models;
using STO.Core.Services;
using STO.Data.Context;

namespace STO.Data.Services;

public class CharacterService(StoDbContext db) : ICharacterService
{
    public async Task<IReadOnlyList<Character>> GetAllAsync()
    {
        return await db.Characters
            .Include(c => c.Account)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Character?> GetByIdAsync(int id)
    {
        return await db.Characters
            .Include(c => c.Account)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IReadOnlyList<Character>> GetByAccountIdAsync(int accountId)
    {
        return await db.Characters
            .Where(c => c.AccountId == accountId)
            .Include(c => c.Account)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Character> CreateAsync(Character character)
    {
        db.Characters.Add(character);
        await db.SaveChangesAsync();
        return character;
    }

    public async Task<Character> UpdateAsync(Character character)
    {
        db.Characters.Update(character);
        await db.SaveChangesAsync();
        return character;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var character = await db.Characters.FindAsync(id);
        if (character is null) return false;

        db.Characters.Remove(character);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<Character?> GetWithFullDetailsAsync(int id)
    {
        return await db.Characters
            .Include(c => c.Account)
            .Include(c => c.Builds).ThenInclude(b => b.Slots).ThenInclude(s => s.Item)
            .Include(c => c.InventoryItems).ThenInclude(i => i.Item)
            .Include(c => c.Reputations)
            .Include(c => c.CharacterTraits).ThenInclude(ct => ct.Trait)
            .Include(c => c.AdmiraltyShips)
            .Include(c => c.DoffAssignments)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
