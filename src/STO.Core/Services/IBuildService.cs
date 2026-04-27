using STO.Core.Models;

namespace STO.Core.Services;

public interface IBuildService
{
    Task<Build?> GetByIdAsync(int id);
    Task<IReadOnlyList<Build>> GetByCharacterIdAsync(int characterId);
    Task<Build> CreateAsync(Build build);
    Task<Build> UpdateAsync(Build build);
    Task<bool> DeleteAsync(int id);
    Task<BuildSlot> AddSlotAsync(BuildSlot slot);
    Task<bool> RemoveSlotAsync(int slotId);
    Task<BuildSlot> UpdateSlotAsync(BuildSlot slot);
    Task<IReadOnlyList<Build>> GetActiveBuildsAsync();
}
