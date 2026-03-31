using System.Security.Claims;
using InvTracker.Logging;
using InvTracker.Models;
using InvTracker.Services;
using InvTracker.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace InvTracker.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ReturnsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IAggregateAccountService _aggregateAccountService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ReturnsController> _logger;

    public ReturnsController(
        IXIRRCalculator xirrCalculator, IMemoryCache cache, ILogger<ReturnsController> logger, IAccountService accountService, IAggregateAccountService aggregateAccountService)
    {
        _cache = cache;
        _logger = logger;
        _accountService = accountService;
        _aggregateAccountService = aggregateAccountService;
    }

    // ACCOUNT IN GIVEN YEAR
    [HttpGet("individualaccountdata/{accountId}/year/{year}")]
    public async Task<ActionResult<AccountYearlyDetail>> GetAccountYearlyDetails(int accountId, int year)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        try
        {
            if (_cache.TryGetValue($"accountyearlyreturns_{userId}_{accountId}_{year}", out AccountYearlyDetail? cachedReturns))
            {
                _logger.CacheUsed("AccountPage");
                return cachedReturns;
            }

            var accountYearlyDetail = await _accountService.GetAccountDetailsByYear(userId, accountId, year);

            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(200));

            _cache.Set($"accountyearlyreturns_{userId}_{accountId}_{year}", accountYearlyDetail, cacheEntryOptions);

            _logger.CacheSet("AccountPage");
            return Ok(accountYearlyDetail);
        }
        catch(Exception)
        {
            return NotFound();
        }
    }

    // HOME PAGE
    [HttpGet("accountreturns")]
    public async Task<ActionResult<HomePageData>> GetAccountReturns()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        if (_cache.TryGetValue($"homePageData_{userId}", out HomePageData? cachedResult))
        {
            _logger.CacheUsed("HomePage");
            return Ok(cachedResult);
        }

        var homePageData = new HomePageData()
        {
            AggregateData = await _aggregateAccountService.GetAggregateData(userId),
            IndividualAccountData = await _aggregateAccountService.GetIndividualAccountsData(userId)
        };

        var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(200));
        _cache.Set($"homePageData_{userId}", homePageData, cacheEntryOptions);
        _logger.CacheSet("HomePage");

        return Ok(homePageData);
    }

    
    



    // This sends the returns to an excel file
    //[HttpGet("generateExcel")]
    //public async Task<ActionResult> SendReturnsToExcelFile()
    //{
    //    var filePath = "C:\\Users\\mattb\\Documents\\TestFiles";


    //    var fileName = $"InvestmentUpdate{DateTime.Now:HHmm-ddMMyyyy}.csv";
    //    var fullFileName = Path.Combine(filePath, fileName);

    //    using var writer = new StreamWriter(fullFileName, false, new UTF8Encoding(true));

    //    var accounts = await _accountsRepo.GetAccounts();

    //    var balances = await _balancesRepo.GetAllBalances();

    //    var payments = await _paymentsRepo.GetAllPayments();



    //    var accounts2 = await _accountsRepo.GetAccounts();

    //    // ***************

    //    writer.WriteLine("Account Name, Jan 2021, 2021, 2022, 2023, 2024, 2025, Cumulative Return");


    //    writer.WriteLine();

    //    foreach (var account in accounts2)
    //    {
    //        var row = new List<double>()
    //        {
    //            fullAccountReturn.AccountReturns[account.Id].Balances[2021].StartingBalance
    //        };

    //        var years = fullAccountReturn.AccountReturns[account.Id].Balances.Select(x => x.Key).ToList();

    //        foreach (var year in years)
    //        {
    //            row.Add(fullAccountReturn.AccountReturns[account.Id].Balances[year].EndBalance);
    //            row.Add(fullAccountReturn.AccountReturns[account.Id].YearlyReturns[year]);
    //        };

    //        var cumRet = fullAccountReturn.AccountReturns[account.Id].CumulativeReturn;

    //        writer.WriteLine($"{account.Name}, £{row[0]}, £{row[1]}, £{row[3]}, £{row[5]}, £{row[7]}, £{row[9]}");
    //        writer.WriteLine();
    //        writer.WriteLine($"XIRR, N/A, %{row[2]}, %{row[4]}, %{row[6]}, %{row[8]}, %{row[10]}, {cumRet}");
    //        writer.WriteLine();
    //    }

    //    var aggBalances = fullAccountReturn.AggregateBalances;
    //    var aggRet = fullAccountReturn.OverallReturns;

    //    writer.WriteLine($"Total, £{aggBalances[2021].StartBalance}, £{aggBalances[2021].EndBalance}, £{aggBalances[2022].EndBalance}, £{aggBalances[2023].EndBalance}, £{aggBalances[2024].EndBalance}, £{aggBalances[2025].EndBalance},");

    //    writer.WriteLine($"XIRR, N/A, %{aggRet[2021]}, %{aggRet[2022]}, %{aggRet[2023]}, %{aggRet[2024]}, %{aggRet[2025]},");

    //    // COULD THEN DO BALANCE PCT CHANGE AND NET DEPOSITS PERCENTAGE

    //    return Ok();
    //}
}