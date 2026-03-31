using InvTracker.Models;
using InvTracker.Repositories;
using InvTracker.Utils;
using Microsoft.AspNetCore.Mvc;

namespace InvTracker.Services;

public interface IAccountService
{
    Task<AccountYearlyDetail> GetAccountDetailsByYear(string userId, int accountId, int year);
}

public class AccountService : IAccountService
{
    private readonly IBalancesRepository _balancesRepo;
    private readonly IPaymentsRepository _paymentsRepo;
    private readonly IXIRRCalculator _xirrCalculator;

    public AccountService(IXIRRCalculator xIRRCalculator, IBalancesRepository balancesRepo, IPaymentsRepository paymentsRepo)
    {
        _balancesRepo = balancesRepo;
        _paymentsRepo = paymentsRepo;
        // SHOULD BE STATIC?
        _xirrCalculator = xIRRCalculator;
    }

    public async Task<AccountYearlyDetail> GetAccountDetailsByYear(string userId, int accountId, int year)
    {
        var balance = await _balancesRepo.GetBalanceByIdAndYearAsync(userId, accountId, year);

        if (balance is null)
        {
            throw new InvalidOperationException();
        }

        var payments = await _paymentsRepo.GetPaymentsByIdAndYearAsync(userId, accountId, year);

        var accountYearlyDetail = new AccountYearlyDetail()
        {
            Balance = balance,
            Payments = payments,
            XIRR = _xirrCalculator.CalculateXirr(balance, payments, year)
        };

        return accountYearlyDetail;

    }
}
