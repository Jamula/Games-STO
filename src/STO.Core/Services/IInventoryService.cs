using STO.Core.Enums;
using STO.Core.Models;

namespace STO.Core.Services;

public interface IInventoryService
{
    Task<IReadOnlyList<InventoryItem>> GetByCharacterIdAsync(int characterId);
    Task<InventoryItem> AddItemAsync(InventoryItem item);
    Task<bool> RemoveItemAsync(int id);
    Task<InventoryItem> UpdateItemAsync(InventoryItem item);
    Task<IReadOnlyList<InventoryItem>> SearchAcrossAccountsAsync(string searchTerm);
    Task<IReadOnlyList<InventoryItem>> GetByLocationAsync(int characterId, InventoryLocation location);
}
