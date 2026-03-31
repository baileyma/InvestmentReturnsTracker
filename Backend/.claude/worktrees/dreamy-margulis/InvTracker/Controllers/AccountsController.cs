using InvTracker.DbContexts;
using InvTracker.Models;
using InvTracker.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            var accounts = await _repo.GetAccounts();

            return Ok(accounts);
        }

        
    }
}