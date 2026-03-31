using InvTracker.Models;
using InvTracker.Repositories;
using InvTracker.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace InvTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UpdateController : ControllerBase
    {
        private readonly IPaymentsRepository _paymentsRepo;
        private readonly IBalancesRepository _balancesRepo;
        private readonly IMemoryCache _cache;

        public UpdateController(IPaymentsRepository paymentsRepo, IBalancesRepository balancesRepo, IMemoryCache cache)
        {
            _paymentsRepo = paymentsRepo;
            _balancesRepo = balancesRepo;
            _cache = cache;
        }

        [HttpPost("add-payment")]
        public async Task<ActionResult> PostPayment(PaymentToAdd payment)
        {
            await _paymentsRepo.PostPayment(payment);

            _cache.Remove($"accountyearlyreturns_{payment.AccountId}_{payment.Year}");
            _cache.Remove("homePageData");

            return Created();
        }

        [HttpPut("update-balance")]
        public async Task<ActionResult> UpsertBalance([FromBody] BalanceToAdd balance)
        {
            await _balancesRepo.PostBalance(balance);// hello

            _cache.Remove($"accountyearlyreturns_{balance.AccountId}_{balance.Year}");
            _cache.Remove("homePageData");

            return NoContent();
        }

        [HttpDelete("delete-payment/{paymentId}")]
        public async Task<ActionResult> DeletePayment([FromRoute] int paymentId)
        {
            await _paymentsRepo.DeletePayment(paymentId);

            _cache.Remove("homePageData");

            return NoContent();
        }


        // learn result methods
        // ok, created/createdataction, notfound, badrequest, nocontent (for put)

        //[HttpPost("add-account")]
        //public async Task<ActionResult<Account>> AddAccount([FromBody] string accountName)
        //{
        //    var newAccount = await _repo.AddAccountAsync(accountName);
        //    await _repo.AddBalancesForAllYears(newAccount.Id);

        //    return CreatedAtAction(nameof(AccountsController.GetAccounts), new { id = newAccount.Id }, newAccount);
        //}

        //[HttpDelete("delete/{id}")]
        //public async Task<ActionResult> DeleteAccount(int accountId)
        //{
        //    await _repo.DeleteAccountById(accountId);

        //    return NoContent();
        //}
    }
}
