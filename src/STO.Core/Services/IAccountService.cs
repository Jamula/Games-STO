using STO.Core.Models;

namespace STO.Core.Services;

public interface IAccountService
{
    Task<IReadOnlyList<Account>> GetAllAsync();
    Task<Account?> GetByIdAsync(int id);
    Task<Account?> GetByGamertagAsync(string gamertag);
    Task<Account> CreateAsync(Account account);
    Task<Account> UpdateAsync(Account account);
    Task<bool> DeleteAsync(int id);
}
