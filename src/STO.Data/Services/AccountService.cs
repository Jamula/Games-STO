using Microsoft.EntityFrameworkCore;
using STO.Core.Models;
using STO.Core.Services;
using STO.Data.Context;

namespace STO.Data.Services;

public class AccountService(StoDbContext db) : IAccountService
{
    public async Task<IReadOnlyList<Account>> GetAllAsync()
    {
        return await db.Accounts
            .Include(a => a.Characters)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        return await db.Accounts
            .Include(a => a.Characters)
            .Include(a => a.ValuableItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Account?> GetByGamertagAsync(string gamertag)
    {
        return await db.Accounts
            .Include(a => a.Characters)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Gamertag == gamertag);
    }

    public async Task<Account> CreateAsync(Account account)
    {
        db.Accounts.Add(account);
        await db.SaveChangesAsync();
        return account;
    }

    public async Task<Account> UpdateAsync(Account account)
    {
        db.Accounts.Update(account);
        await db.SaveChangesAsync();
        return account;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var account = await db.Accounts.FindAsync(id);
        if (account is null) return false;

        db.Accounts.Remove(account);
        await db.SaveChangesAsync();
        return true;
    }
}
