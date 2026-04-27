using Microsoft.EntityFrameworkCore;
using STO.Core.Models;
using STO.Core.Services;
using STO.Data.Context;

namespace STO.Data.Services;

public class ValuableItemService(StoDbContext db) : IValuableItemService
{
    public async Task<IReadOnlyList<ValuableItem>> GetAllAsync()
    {
        return await db.ValuableItems
            .Include(v => v.Account)
            .Include(v => v.Character)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<ValuableItem>> GetByAccountIdAsync(int accountId)
    {
        return await db.ValuableItems
            .Where(v => v.AccountId == accountId)
            .Include(v => v.Character)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ValuableItem> AddAsync(ValuableItem item)
    {
        db.ValuableItems.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    public async Task<ValuableItem> UpdateAsync(ValuableItem item)
    {
        db.ValuableItems.Update(item);
        await db.SaveChangesAsync();
        return item;
    }

    public async Task<bool> RemoveAsync(int id)
    {
        var item = await db.ValuableItems.FindAsync(id);
        if (item is null) return false;

        db.ValuableItems.Remove(item);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<ValuableItem>> SearchAsync(string searchTerm)
    {
        return await db.ValuableItems
            .Include(v => v.Account)
            .Include(v => v.Character)
            .Where(v => EF.Functions.Like(v.Name, $"%{searchTerm}%"))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyDictionary<string, int>> GetSummaryAsync()
    {
        return await db.ValuableItems
            .GroupBy(v => v.ItemType.ToString())
            .ToDictionaryAsync(g => g.Key, g => g.Sum(v => v.Quantity));
    }
}
