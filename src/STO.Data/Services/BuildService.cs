using Microsoft.EntityFrameworkCore;
using STO.Core.Models;
using STO.Core.Services;
using STO.Data.Context;

namespace STO.Data.Services;

public class BuildService(StoDbContext db) : IBuildService
{
    public async Task<Build?> GetByIdAsync(int id)
    {
        return await db.Builds
            .Include(b => b.Slots).ThenInclude(s => s.Item)
            .Include(b => b.Character)
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IReadOnlyList<Build>> GetByCharacterIdAsync(int characterId)
    {
        return await db.Builds
            .Where(b => b.CharacterId == characterId)
            .Include(b => b.Slots).ThenInclude(s => s.Item)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Build> CreateAsync(Build build)
    {
        db.Builds.Add(build);
        await db.SaveChangesAsync();
        return build;
    }

    public async Task<Build> UpdateAsync(Build build)
    {
        db.Builds.Update(build);
        await db.SaveChangesAsync();
        return build;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var build = await db.Builds.FindAsync(id);
        if (build is null) return false;

        db.Builds.Remove(build);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<BuildSlot> AddSlotAsync(BuildSlot slot)
    {
        db.BuildSlots.Add(slot);
        await db.SaveChangesAsync();
        return slot;
    }

    public async Task<bool> RemoveSlotAsync(int slotId)
    {
        var slot = await db.BuildSlots.FindAsync(slotId);
        if (slot is null) return false;

        db.BuildSlots.Remove(slot);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<BuildSlot> UpdateSlotAsync(BuildSlot slot)
    {
        db.BuildSlots.Update(slot);
        await db.SaveChangesAsync();
        return slot;
    }

    public async Task<IReadOnlyList<Build>> GetActiveBuildsAsync()
    {
        return await db.Builds
            .Where(b => b.IsActive)
            .Include(b => b.Slots).ThenInclude(s => s.Item)
            .Include(b => b.Character)
            .AsNoTracking()
            .ToListAsync();
    }
}
