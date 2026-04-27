using Microsoft.EntityFrameworkCore;
using STO.Core.Enums;
using STO.Core.Models;
using STO.Core.Services;
using STO.Data.Context;

namespace STO.Data.Services;

public class InventoryService(StoDbContext db) : IInventoryService
{
    public async Task<IReadOnlyList<InventoryItem>> GetByCharacterIdAsync(int characterId)
    {
        return await db.InventoryItems
            .Where(i => i.CharacterId == characterId)
            .Include(i => i.Item)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<InventoryItem> AddItemAsync(InventoryItem item)
    {
        db.InventoryItems.Add(item);
        await db.SaveChangesAsync();
        return item;
    }

    public async Task<bool> RemoveItemAsync(int id)
    {
        var item = await db.InventoryItems.FindAsync(id);
        if (item is null) return false;

        db.InventoryItems.Remove(item);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<InventoryItem> UpdateItemAsync(InventoryItem item)
    {
        db.InventoryItems.Update(item);
        await db.SaveChangesAsync();
        return item;
    }

    public async Task<IReadOnlyList<InventoryItem>> SearchAcrossAccountsAsync(string searchTerm)
    {
        return await db.InventoryItems
            .Include(i => i.Item)
            .Include(i => i.Character).ThenInclude(c => c.Account)
            .Where(i => EF.Functions.Like(i.Item.Name, $"%{searchTerm}%"))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<InventoryItem>> GetByLocationAsync(int characterId, InventoryLocation location)
    {
        return await db.InventoryItems
            .Where(i => i.CharacterId == characterId && i.Location == location)
            .Include(i => i.Item)
            .AsNoTracking()
            .ToListAsync();
    }
}
