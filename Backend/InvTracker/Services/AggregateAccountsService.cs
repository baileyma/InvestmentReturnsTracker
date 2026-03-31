using InvTracker.Models;
using InvTracker.Repositories;
using InvTracker.Utils;

namespace InvTracker.Services;

public interface IAggregateAccountService
{
    Task<Dictionary<int, AggregateData>> GetAggregateData(string userId);

    Task<Dictionary<int, AccountReturn>> GetIndividualAccountsData(string userId);
}

public class AggregateAccountsService : IAggregateAccountService
{
    private readonly IAccountsRepository _accountsRepo;
    private readonly IBalancesRepository _balancesRepo;
    private readonly IPaymentsRepository _paymentsRepo;
    private readonly IXIRRCalculator _xirrCalculator;

    public AggregateAccountsService(IAccountsRepository accountsRepo, IBalancesRepository balancesRepo, IPaymentsRepository paymentsRepo, IXIRRCalculator xirrCalculator)
    {
        _accountsRepo = accountsRepo;
        _balancesRepo = balancesRepo;
        _paymentsRepo = paymentsRepo;
        _xirrCalculator = xirrCalculator;
    }   

    public async Task<Dictionary<int, AggregateData>> GetAggregateData(string userId)
    {
        // DRY
        var years = Enumerable.Range(2021, DateTime.Today.Year - 2020);
        var accounts = await _accountsRepo.GetAccountsByUserId(userId);
        var balances = await _balancesRepo.GetBalancesByUserIdAsync(userId);
        var payments = await _paymentsRepo.GetAllPaymentsAsync(userId);

        var balancesLookup = balances.
            GroupBy(x => x.Year).
            ToDictionary(g => g.Key, g => g.ToList());

        var paymentsLookup = payments.
            GroupBy(x => x.Date.Year).
            ToDictionary(g => g.Key, x => x.ToList());
        // PROBABLY WANT TO SPLIT INTO TWO METHODS
        //  1 IS THE AGGREGATE ACCOUNT DATA SHOWN BELOW ON HOME PAGE

        var aggDataAllYears = new Dictionary<int, AggregateData>();

        foreach (var year in years)
        {
            var balancesForYear = balancesLookup[year];
            var paymentsForYear = paymentsLookup[year];

            // GET RID
            var overallBalance = new MinimalBalance
            {
                StartingBalance = balancesForYear.Sum(x => x.StartingBalance),
                EndBalance = balancesForYear.Sum(x => x.EndBalance),
                Day = balancesForYear.Min(x => x.Day),
                Month = balancesForYear.Min(x => x.Month),
                Year = year
            };

            var aggDataEntry = new AggregateData()
            {
                XIRR = _xirrCalculator.CalculateXirr(overallBalance, paymentsForYear, year),
                NetDeposits = paymentsForYear.Sum(x => x.Amount),
                TotalWithdrawals = paymentsForYear.Where(x => x.Amount < 0).Sum(x => x.Amount),
                TotalDeposits = paymentsForYear.Where(x => x.Amount > 0).Sum(x => x.Amount),
                StartBalance = balancesForYear.Sum(x => x.StartingBalance),
                EndBalance = balancesForYear.Sum(x => x.EndBalance)
            };

            aggDataAllYears.Add(year, aggDataEntry);

        }
        return aggDataAllYears;
    }


    public async Task<Dictionary<int, AccountReturn>> GetIndividualAccountsData(string userId)
    {
        var years = Enumerable.Range(2021, DateTime.Today.Year - 2020);
        var accounts = await _accountsRepo.GetAccountsByUserId(userId);
        var balances = await _balancesRepo.GetBalancesByUserIdAsync(userId);
        var payments = await _paymentsRepo.GetAllPaymentsAsync(userId);

        var individualAccountData = new Dictionary<int, AccountReturn>();

        foreach (var item in accounts)
        {
            var accountReturn = new AccountReturn()
            {
                AccountId = item.Id,
                AccountName = item.Name,
                BalancesAndReturnsByYear = GetAccountYearlyData(item.Id, balances, payments, years)
            };
            individualAccountData.Add(item.Id, accountReturn);
        }
        return individualAccountData;
    }

    // change that return object
    private Dictionary<int, BalanceAndXIRR> GetAccountYearlyData(int accountId, List<Balance> balances, List<Payment> payments, IEnumerable<int> years)
    {
        var paymentsLookupByIdAndYear = payments.
            GroupBy(x => x.AccountId).
            ToDictionary(g => g.Key, g => g.GroupBy(x => x.Date.Year).ToDictionary(h => h.Key, h => h.ToList()));

        // something wrong with inner dictionary having to zero index
        var balancesLookupByIdAndYear = balances.
            GroupBy(x => x.AccountId).
            ToDictionary(g => g.Key, g => g.GroupBy(x => x.Year).ToDictionary(x => x.Key, x => x.ToList()[0]));

        var balanceAndXIRRByYear = new Dictionary<int, BalanceAndXIRR>();

        foreach (var year in years)
        {
            var accountBalance = balancesLookupByIdAndYear[accountId][year];

            if (!paymentsLookupByIdAndYear[accountId].TryGetValue(year, out var accountPayments))
            {
                accountPayments = new List<Payment>();
            }

            var xirr = _xirrCalculator.CalculateXirr(accountBalance, accountPayments, year);
            var balanceAndXIRR = new BalanceAndXIRR()
            {
                Year = year,
                StartingBalance = accountBalance.StartingBalance,
                EndBalance = accountBalance.EndBalance,
                Month = accountBalance.Month,
                Day = accountBalance.Day,
                XIRR = xirr
            };

            balanceAndXIRRByYear.Add(year, balanceAndXIRR);
        }

        return balanceAndXIRRByYear;
    }
}
