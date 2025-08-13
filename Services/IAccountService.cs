
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GemachApp.Data;


namespace GemachApp.Services
{
    public interface IAccountService
    {
        Task<decimal> UpdateAccountBalanceForClient(int clientId);
        Task<decimal> CalculateClientBalance(int clientId);
        Task RecalculateRunningTotals(int clientId);
    }

    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;

        public AccountService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalculateClientBalance(int clientId)
        {
            // Get all transactions for this client
            var transactions = await _context.Transactions
                .Where(t => t.ClientId == clientId)
                .ToListAsync();

            // Calculate the net balance (Added - Subtracted)
            decimal balance = transactions.Sum(t => (t.Added ?? 0) - (t.Subtracted ?? 0));

            return balance;
        }

        public async Task<decimal> UpdateAccountBalanceForClient(int clientId)
        {
            // Calculate current balance based on transactions
            decimal balance = await CalculateClientBalance(clientId);

            // Find the account for the client
            var account = await _context.Accounts
                .AsTracking()
                .FirstOrDefaultAsync(a => a.ClientId == clientId);

            if (account != null)
            {
                // Update existing account
                account.TotalAmount = balance;
                account.UpdateBalDate = DateTime.Now;
                _context.Entry(account).State = EntityState.Modified;
            }
            else
            {
                // Create new account if it doesn't exist
                account = new Account
                {
                    ClientId = clientId,
                    TotalAmount = balance,
                    UpdateBalDate = DateTime.Now
                };
                _context.Accounts.Add(account);
            }

            await _context.SaveChangesAsync();
            return balance;
        }

        public async Task RecalculateRunningTotals(int clientId)
        {
            // Get all transactions for this client in date order
            var transactions = await _context.Transactions
                .Where(t => t.ClientId == clientId)
                .OrderBy(t => t.TransDate)
                .ToListAsync();

            // Recalculate running totals
            decimal runningAdded = 0;
            decimal runningSubtracted = 0;

            foreach (var t in transactions)
            {
                runningAdded += t.Added ?? 0;
                runningSubtracted += t.Subtracted ?? 0;

                t.TotalAdded = runningAdded;
                t.TotalSubtracted = runningSubtracted;
            }

            // Update transactions in database
            if (transactions.Any())
            {
                _context.Transactions.UpdateRange(transactions);
                await _context.SaveChangesAsync();
            }
        }
    }
}



