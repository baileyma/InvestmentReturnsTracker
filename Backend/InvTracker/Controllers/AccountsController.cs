using System.Security.Claims;
using InvTracker.Models;
using InvTracker.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountsRepository _repo;

        public AccountsController(IAccountsRepository repo)
        {
            _repo = repo;
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var accounts = await _repo.GetAccountsByUserId(userId);

            return Ok(accounts);
        }


    }
}