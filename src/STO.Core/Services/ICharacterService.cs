using STO.Core.Models;

namespace STO.Core.Services;

public interface ICharacterService
{
    Task<IReadOnlyList<Character>> GetAllAsync();
    Task<Character?> GetByIdAsync(int id);
    Task<IReadOnlyList<Character>> GetByAccountIdAsync(int accountId);
    Task<Character> CreateAsync(Character character);
    Task<Character> UpdateAsync(Character character);
    Task<bool> DeleteAsync(int id);
    Task<Character?> GetWithFullDetailsAsync(int id);
}
