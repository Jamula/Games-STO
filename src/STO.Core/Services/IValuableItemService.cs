using STO.Core.Models;

namespace STO.Core.Services;

public interface IValuableItemService
{
    Task<IReadOnlyList<ValuableItem>> GetAllAsync();
    Task<IReadOnlyList<ValuableItem>> GetByAccountIdAsync(int accountId);
    Task<ValuableItem> AddAsync(ValuableItem item);
    Task<ValuableItem> UpdateAsync(ValuableItem item);
    Task<bool> RemoveAsync(int id);
    Task<IReadOnlyList<ValuableItem>> SearchAsync(string searchTerm);
    Task<IReadOnlyDictionary<string, int>> GetSummaryAsync();
}
