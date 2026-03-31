using InvTracker.DbContexts;
using InvTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace InvTracker.Repositories;

public interface IAccountsRepository
{
    Task<List<Account>> GetAccountsByUserId(string userId);
    Task<Account?> GetAccountByIdAsync(string userId, int id);
    Task<Account> AddAccountAsync(string userId, string accountName);
    Task DeleteAccountByIdAsync(string userId, int accountId);
}

public class AccountsRepository : IAccountsRepository
{
    private InvTrackerContext _context;
    public AccountsRepository(InvTrackerContext context)
    {
        _context = context;   
    }

    public async Task<List<Account>> GetAccountsByUserId(string userId)
    {
        return await _context.Accounts.Where(x => x.UserId == userId).ToListAsync();
    }

    public async Task<Account?> GetAccountByIdAsync(string userId, int id)
    {
        var userAccounts = _context.Accounts.Where(x => x.UserId == userId);
        return await userAccounts.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Account> AddAccountAsync(string userId, string accountName)
    {
        var newAccount = new Account() 
        {
            UserId = userId,
            Name = accountName 
        };
        _context.Accounts.Add(newAccount);

        await _context.SaveChangesAsync();

        return newAccount;

    }

    public async Task DeleteAccountByIdAsync(string userId, int accountId)
    {
        var accountToDelete = await GetAccountByIdAsync(userId, accountId);

        if (accountToDelete is not null)
        {
            _context.Accounts.Remove(accountToDelete);

        }
        await _context.SaveChangesAsync();
    }
}
