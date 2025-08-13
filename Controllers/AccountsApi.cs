using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GemachApp.Data;
using GemachApp.Services;
using System.Threading.Tasks;

namespace GemachApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAccountService _accountService;

        public AccountsApiController(AppDbContext context, IAccountService accountService)
        {
            _context = context;
            _accountService = accountService;
        }

        // GET: api/AccountsApi/GetBalance/5
        [HttpGet("GetBalance/{clientId}")]
        public async Task<IActionResult> GetBalance(int clientId)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.ClientId == clientId);
            if (account == null)
                return NotFound("Client not found");
           
                return Ok(account.TotalAmount);
            
        }

        // POST: api/AccountsApi/resync-balances
        [HttpPost("resync-balances")]
        public async Task<IActionResult> ResyncAccountBalances()
        {
            var allClients = await _context.Clients.ToListAsync();
            int updatedCount = 0;

            foreach (var client in allClients)
            {
                await _accountService.UpdateAccountBalanceForClient(client.ClientId);
                updatedCount++;
            }

            return Ok(new { message = $"Balances resynced for {updatedCount} clients." });
        }

        // GET: api/AccountsApi/ValidateFunds/20?amount=1234.56
        [HttpGet("ValidateFunds/{clientId}")]
        public async Task<IActionResult> ValidateFunds(int clientId, [FromQuery] decimal amount)
        {
            if (amount <= 0)
                return BadRequest(new { message = "Amount must be greater than zero." });

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.ClientId == clientId);
            if (account == null)
                return NotFound(new { message = "Account not found." });

            bool hasFunds = account.TotalAmount >= amount;
            return Ok(new { hasFunds });
        }
    }
}