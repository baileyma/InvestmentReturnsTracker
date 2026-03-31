using InvTracker.DbContexts;
using InvTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace InvTracker.Repositories
{
    public interface IBalancesRepository
    {
        Task<List<Balance>> GetAllBalances();
        Task<List<Balance>> GetBalancesById(int accountId);
        Task<Balance?> GetBalanceByIdAndYear(int accountId, int year);
        Task AddBalancesForAllYears(int accountId);
        Task PostBalance(BalanceToAdd balance);
    }

    public class BalancesRepository : IBalancesRepository
    {
        private readonly InvTrackerContext _context;

        public BalancesRepository(InvTrackerContext context)
        {
            _context = context;
        }

        public async Task<List<Balance>> GetAllBalances()
        {
            var balances = await _context.Balances.ToListAsync();

            return balances;
        }

        public async Task<List<Balance>> GetBalancesById(int accountId)
        {
            var balances = await _context.Balances.ToListAsync();

            return balances.Where(x => x.AccountId == accountId).ToList();
        }
        public async Task<Balance?> GetBalanceByIdAndYear(int accountId, int year)
        {
            var balances = await _context.Balances.ToListAsync();

            var balance = balances.Where(x => x.AccountId == accountId)
                .FirstOrDefault(x => x.Year == year);

            if (balance is null)
            {
                return null;
            }

            return balance;
        }
        public async Task AddBalancesForAllYears(int accountId)
        {
            for (int yearCount = 2021; yearCount < DateTime.Today.Year; yearCount++)
            {
                var balanceToAdd = new Balance() { AccountId = accountId, Year = yearCount, StartingBalance = 0, EndBalance = 0, Month = 1, Day = 2 };
                await _context.Balances.AddAsync(balanceToAdd);
            };
            await _context.SaveChangesAsync();
        }

        public async Task PostBalance(BalanceToAdd balance)
        {
            var balanceToUpdate = await _context.Balances.FirstOrDefaultAsync(x => x.AccountId == balance.AccountId && x.Year == balance.Year);

            if (balanceToUpdate is not null)
            {
                balanceToUpdate.EndBalance = balance.LatestBalance;
                balanceToUpdate.Month = balance.Month;
                balanceToUpdate.Day = balance.Day;
            }


            await _context.SaveChangesAsync();
        }






    }
}
