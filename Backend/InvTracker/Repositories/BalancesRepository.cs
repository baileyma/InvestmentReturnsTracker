using InvTracker.DbContexts;
using InvTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace InvTracker.Repositories;

public interface IBalancesRepository
{
    Task<List<Balance>> GetBalancesByUserIdAsync(string userId);
    Task<List<Balance>> GetBalancesByIdAsync(string userId, int accountId);
    Task<Balance?> GetBalanceByIdAndYearAsync(string userId, int accountId, int year);
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

    public async Task<List<Balance>> GetBalancesByUserIdAsync(string userId)
    {
        var userAccounts = _context.Accounts.Where(x => x.UserId == userId).ToList();

        var userAccountIds = userAccounts.Select(x => x.Id).ToHashSet();

        var balances = _context.Balances.Where(x => userAccountIds.Contains(x.AccountId)).ToList();

        return balances;
    }

    // THESE MIGHT NOT NEED USERID LOGIC...ACCOUNTID SHOULD BE FINE
    public async Task<List<Balance>> GetBalancesByIdAsync(string userId, int accountId)
    {
        var allBalancesByUser = await GetBalancesByUserIdAsync(userId);

        return allBalancesByUser.Where(x => x.AccountId == accountId).ToList();
    }

    // THESE MIGHT NOT NEED USERID LOGIC...ACCOUNTID SHOULD BE FINE
    public async Task<Balance?> GetBalanceByIdAndYearAsync(string userId, int accountId, int year)
    {
        var allBalancesByUser = await GetBalancesByUserIdAsync(userId);

        var balance = allBalancesByUser.Where(x => x.AccountId == accountId)
            .FirstOrDefault(x => x.Year == year);

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
