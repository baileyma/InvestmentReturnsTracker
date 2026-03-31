using InvTracker.DbContexts;
using InvTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace InvTracker.Repositories
{
    public interface IAccountsRepository
    {
        Task<List<Account>> GetAccounts();
        Task<Account?> GetAccountById(int id);
        Task<Account> AddAccountAsync(string accountName);
        Task DeleteAccountById(int accountId);
    }

    public class AccountsRepository : IAccountsRepository
    {
        private InvTrackerContext _context;
        public AccountsRepository(InvTrackerContext context)
        {
            _context = context;   
        }

        public async Task<List<Account>> GetAccounts()
        {
            return await _context.Accounts.ToListAsync();
        }

        public async Task<Account?> GetAccountById(int id)
        {
            return await _context.Accounts.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Account> AddAccountAsync(string accountName)
        {
            var newAccount = new Account() { Name = accountName };
            _context.Accounts.Add(newAccount);

            await _context.SaveChangesAsync();

            return newAccount;

        }

        public async Task DeleteAccountById(int accountId)
        {
            var accountToDelete = await GetAccountById(accountId);

            if (accountToDelete is not null)
            {
                _context.Accounts.Remove(accountToDelete);

            }
            await _context.SaveChangesAsync();

        }
    }
}
