using GemachApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;


namespace GemachApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public TransactionsController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: api/Transactions
        [HttpGet]
        public async Task<IActionResult> GetTransactions()
        {
            var transactions = await _context.Transactions
                .Include(t => t.Client)
                .OrderBy(t => t.TransDate)
                .Select(t => new
                {
                    t.TransId,
                    t.ClientId,
                    ClientName = $"{t.Client.ClientFirstName} {t.Client.ClientLastName}",
                    t.TransDate,
                    t.Added,
                    t.Subtracted,
                    t.TotalAdded,
                    t.TotalSubtracted,
                    t.Agent,
                    Balance = _context.Transactions
                        .Where(tr => tr.ClientId == t.ClientId && tr.TransDate <= t.TransDate)
                        .Sum(tr => (tr.Added ?? 0) - (tr.Subtracted ?? 0))
                })
                .ToListAsync();

            return Ok(transactions);
        }

        // GET: api/Transactions/Details/{ClientId}
        [HttpGet("Details/{ClientId}")]
        public async Task<IActionResult> Details(int ClientId)
        {
            var transactions = await _context.Transactions
                .Where(t => t.ClientId == ClientId && (t.Added != null || t.Subtracted != null))
                .Include(t => t.Client)
                .Select(t => new
                {
                    t.TransId,
                    t.ClientId,
                    ClientName = $"{t.Client.ClientFirstName} {t.Client.ClientLastName}",
                    t.TransDate,
                    t.Added,
                    t.Subtracted,
                    t.TotalAdded,
                    t.TotalSubtracted,
                    t.Agent,
                })
                .ToListAsync();

            if (!transactions.Any())
                return NotFound("No transactions found for this client.");

            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.ClientId == ClientId);

            return Ok(new
            {
                Transactions = transactions,
                Balance = account != null ? account.TotalAmount : 0
            });
        }

        // POST: api/Transactions/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] Transaction transaction)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                transaction.TransDate = DateTime.Now;

                if (string.IsNullOrWhiteSpace(transaction.Agent))
                {
                    transaction.Agent = "Unknown"; // Or extract from User.Identity if you have authentication
                }

                _context.Transactions.Add(transaction);

                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.ClientId == transaction.ClientId);

                if (account != null)
                {
                    // Step 1: Update balance
                    account.TotalAmount += transaction.Added ?? 0;
                    account.TotalAmount -= transaction.Subtracted ?? 0;

                    // Step 2: Update balance date
                    account.UpdateBalDate = DateTime.Now;

                    _context.Accounts.Update(account);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Message = "Transaction created successfully.",
                    transaction.Added,
                    transaction.Subtracted
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating transaction: {ex.Message}");
                return StatusCode(500, "An error occurred while creating the transaction.");
            }
        }

        // PUT: api/Transactions/Edit/{TransId}
        [HttpPut("Edit/{TransId}")]
        public async Task<IActionResult> Edit(int TransId, [FromBody] TransactionEditRequest updatedTransaction)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Input validation
            if ((updatedTransaction.Added ?? 0) < 0 || (updatedTransaction.Subtracted ?? 0) < 0)
                return BadRequest("Amounts cannot be negative");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingTransaction = await _context.Transactions.FindAsync(TransId);
                if (existingTransaction == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound("Transaction not found");
                }

                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.ClientId == existingTransaction.ClientId);
                if (account == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound("Account not found for this client");
                }

                // Calculate balance impact
                decimal oldImpact = (existingTransaction.Added ?? 0) - (existingTransaction.Subtracted ?? 0);
                decimal newImpact = (updatedTransaction.Added ?? 0) - (updatedTransaction.Subtracted ?? 0);
                decimal balanceChange = newImpact - oldImpact;

                // Check for negative balance
                if (account.TotalAmount + balanceChange < 0)
                {
                    await transaction.RollbackAsync();
                    return BadRequest("Update would result in negative balance");
                }

                // Update transaction
                existingTransaction.Added = updatedTransaction.Added;
                existingTransaction.Subtracted = updatedTransaction.Subtracted;
                existingTransaction.TransDate = DateTime.UtcNow;

                // Update account balance
                account.TotalAmount += balanceChange;
                account.UpdateBalDate = DateTime.UtcNow;

                // Recalculate cumulative totals for all client transactions
                await RecalculateCumulativeTotals(existingTransaction.ClientId);

                _context.Transactions.Update(existingTransaction);
                _context.Accounts.Update(account);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Transaction updated successfully",
                    updatedBalance = account.TotalAmount
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.Error.WriteLine($"Error editing transaction: {ex.Message}");
                return StatusCode(500, "An error occurred while editing the transaction");
            }
        }
        

        // DELETE: api/Transactions/Delete/{TransId}
        [HttpDelete("Delete/{TransId}")]
        public async Task<IActionResult> Delete(int TransId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var transactionToDelete = await _context.Transactions.FindAsync(TransId);
                if (transactionToDelete == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound("Transaction not found");
                }

                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.ClientId == transactionToDelete.ClientId);
                if (account == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound("Account not found for this client");
                }

                // Calculate impact and check for negative balance
                decimal impact = (transactionToDelete.Added ?? 0) - (transactionToDelete.Subtracted ?? 0);
                if (account.TotalAmount - impact < 0)
                {
                    await transaction.RollbackAsync();
                    return BadRequest("Deleting this transaction would result in negative balance");
                }

                // Update account balance
                account.TotalAmount -= impact;
                account.UpdateBalDate = DateTime.UtcNow;

                // Remove transaction
                _context.Transactions.Remove(transactionToDelete);
                _context.Accounts.Update(account);

                // Recalculate cumulative totals for remaining transactions
                await RecalculateCumulativeTotals(transactionToDelete.ClientId);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Transaction deleted successfully",
                    updatedBalance = account.TotalAmount
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.Error.WriteLine($"Error deleting transaction: {ex.Message}");
                return StatusCode(500, "An error occurred while deleting the transaction");
            }
        }
        

        // Helper method to recalculate cumulative totals
        private async Task RecalculateCumulativeTotals(int clientId)
        {
            var allClientTransactions = await _context.Transactions
                .Where(t => t.ClientId == clientId)
                .OrderBy(t => t.TransDate)
                .ThenBy(t => t.TransId)
                .ToListAsync();

            decimal runningTotalAdded = 0;
            decimal runningTotalSubtracted = 0;

            foreach (var t in allClientTransactions)
            {
                runningTotalAdded += t.Added ?? 0;
                runningTotalSubtracted += t.Subtracted ?? 0;

                t.TotalAdded = runningTotalAdded;
                t.TotalSubtracted = runningTotalSubtracted;
            }

            _context.Transactions.UpdateRange(allClientTransactions);
        }

        // GET: api/Transactions/GetTransactionsByClient/{clientId}
        [HttpGet("GetTransactionsByClient/{clientId}")]
        public async Task<IActionResult> GetTransactionsByClient(int clientId)
        {
            // Validate client exists first
            var clientExists = await _context.Clients.AnyAsync(c => c.ClientId == clientId);
            if (!clientExists)
            {
                return NotFound("Client not found");
            }

            var transactions = await _context.Transactions
                .Where(t => t.ClientId == clientId)
                .OrderBy(t => t.TransDate)
                .ToListAsync();

            if (!transactions.Any())
            {
                return Ok(new { Message = "No transactions found", Transactions = new List<object>() });
            }

            // Calculate running balance server-side - THIS IS THE KEY FIX
            decimal runningBalance = 0;
            var result = transactions.Select(t =>
            {
                runningBalance += (t.Added ?? 0) - (t.Subtracted ?? 0);
                return new
                {
                    transId = t.TransId,
                    clientId = t.ClientId,
                    transDate = t.TransDate,
                    added = t.Added ?? 0,
                    subtracted = t.Subtracted ?? 0,
                    agent = t.Agent ?? "Unknown",
                    runningBalance = runningBalance // This matches what your frontend expects
                };
            }).ToList();

            return Ok(result);
        }


        // POST: api/Transactions/ProcessTransaction
        [HttpPost("ProcessTransaction")]
        public async Task<IActionResult> ProcessTransaction([FromBody] TransactionRequest request)
        {
            if (request == null)
                return BadRequest("Transaction data is missing");

            // Input validation
            if (request.ClientId <= 0)
                return BadRequest("Valid Client ID is required");

            if (string.IsNullOrWhiteSpace(request.Agent))
                return BadRequest("Agent name is required");

            // Validate that at least one amount is provided
            if ((request.Added ?? 0) == 0 && (request.Subtracted ?? 0) == 0)
                return BadRequest("Either Added or Subtracted amount must be greater than 0");

            // Validate amounts are not negative
            if ((request.Added ?? 0) < 0 || (request.Subtracted ?? 0) < 0)
                return BadRequest("Amounts cannot be negative");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Validate client exists
                var client = await _context.Clients.FindAsync(request.ClientId);
                if (client == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound("Client not found");
                }

                // Process the amounts
                decimal addAmount = request.Added ?? 0;
                decimal subtractAmount = request.Subtracted ?? 0;

                // 2. Get or create account
                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.ClientId == request.ClientId);

                if (account == null)
                {
                    account = new Account
                    {
                        ClientId = request.ClientId,
                        TotalAmount = 0,
                        UpdateBalDate = DateTime.UtcNow
                    };
                    _context.Accounts.Add(account);
                    await _context.SaveChangesAsync(); // Save to get the AccountId
                }

                // 3. Check for negative balance (business rule)
                decimal currentBalance = account.TotalAmount ?? 0;
                decimal newBalance = currentBalance + addAmount - subtractAmount;
                if (newBalance < 0)
                {
                    await transaction.RollbackAsync();
                    return BadRequest($"Transaction would result in negative balance. Current: {currentBalance:C}, Requested: +{addAmount:C} -{subtractAmount:C}");
                }

                // 4. Calculate cumulative totals for the new transaction
                var existingTotals = await _context.Transactions
                    .Where(t => t.ClientId == request.ClientId)
                    .Select(t => new {
                        Added = t.Added ?? 0,
                        Subtracted = t.Subtracted ?? 0
                    })
                    .ToListAsync();

                decimal totalAdded = existingTotals.Sum(t => t.Added);
                decimal totalSubtracted = existingTotals.Sum(t => t.Subtracted);

                // 5. Create new transaction with server-calculated totals
                var newTransaction = new Transaction
                {
                    ClientId = request.ClientId,
                    Added = addAmount > 0 ? addAmount : null,
                    Subtracted = subtractAmount > 0 ? subtractAmount : null,
                    TransDate = DateTime.UtcNow,
                    Agent = request.Agent.Trim(),
                    TotalAdded = totalAdded + addAmount,
                    TotalSubtracted = totalSubtracted + subtractAmount
                };

                _context.Transactions.Add(newTransaction);

                // 6. Update account balance (server-side calculation)
                account.TotalAmount = newBalance;
                account.UpdateBalDate = DateTime.UtcNow;
                _context.Accounts.Update(account);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 7. Send email if requested
                if (request.SendEmail == true && !string.IsNullOrEmpty(client.Email))
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            client.Email,
                            "Transaction Confirmation",
                            $"Dear {client.ClientFirstName},\n\n" +
                            $"Your transaction has been processed:\n" +
                            $"Added: {(addAmount > 0 ? $"${addAmount:F2}" : "N/A")}\n" +
                            $"Subtracted: {(subtractAmount > 0 ? $"${subtractAmount:F2}" : "N/A")}\n" +
                            $"New Balance: ${account.TotalAmount:F2}\n\n" +
                            $"Transaction processed by: {request.Agent}\n" +
                            $"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
                        );
                    }
                    catch (Exception emailEx)
                    {
                        // Log email error but don't fail the transaction
                        Console.WriteLine($"Email sending failed: {emailEx.Message}");
                    }
                }

                return Ok(new
                {
                    message = "Transaction processed successfully",
                    transactionId = newTransaction.TransId,
                    updatedBalance = account.TotalAmount,
                    added = addAmount,
                    subtracted = subtractAmount,
                    agent = request.Agent,
                    transactionDate = newTransaction.TransDate
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.Error.WriteLine($"Error processing transaction: {ex.Message}");
                return StatusCode(500, "An error occurred while processing the transaction");
            }
        }


        // DTO class for transaction requests
        public class TransactionRequest
        {
            public int ClientId { get; set; }
            public decimal? Added { get; set; }
            public decimal? Subtracted { get; set; }
            public bool? SendEmail { get; set; }
            [Required]
            public string Agent { get; set; }
        }

        public class TransactionEditRequest
        {
            public decimal? Added { get; set; }
            public decimal? Subtracted { get; set; }
        }

        [HttpPost("GetTransactionsByPassword")]
        public async Task<IActionResult> GetTransactionsByPassword([FromBody] string password)
        {
            // First, check if a client exists with this password
            var clientExists = await _context.Clients.AnyAsync(c => c.ClientPassword == password);

            if (!clientExists)
            {
                return NotFound("Client with this password does not exist.");
            }

            // Now get the transactions
            var transactions = await _context.Transactions
                .Include(t => t.Client)
                .Where(t => t.Client.ClientPassword == password)
                .OrderBy(t => t.TransDate)
                .Select(t => new
                {
                    t.TransId,
                    t.ClientId,
                    ClientName = $"{t.Client.ClientFirstName} {t.Client.ClientLastName}",
                    t.TransDate,
                    t.Added,
                    t.Subtracted,
                    t.TotalAdded,
                    t.TotalSubtracted,
                    t.Agent,
                    Balance = _context.Transactions
                        .Where(tr => tr.ClientId == t.ClientId && tr.TransDate <= t.TransDate)
                        .Sum(tr => (tr.Added ?? 0) - (tr.Subtracted ?? 0))
                })
                .ToListAsync();

            // If client exists but has no transactions
            if (transactions.Count == 0)
            {
                return Ok(new { Message = "Client found but has no transactions.", Transactions = new List<object>() });
            }
            return Ok(transactions);
        }

        // Helper methods
        private async Task<bool> ClientExists(int clientId)
        {
            return await _context.Clients.AnyAsync(c => c.ClientId == clientId);
        }

        private async Task<Account> GetOrCreateAccountAsync(int clientId)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.ClientId == clientId);

            if (account == null)
            {
                account = new Account
                {
                    ClientId = clientId,
                    TotalAmount = 0,
                    UpdateBalDate = DateTime.UtcNow
                };

                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();
            }

            return account;
        }


        // GET: api/Transactions/GetClientTransactionsWithRunningBalance/{clientId}
        [HttpGet("GetClientTransactionsWithRunningBalance/{clientId}")]
        public async Task<IActionResult> GetClientTransactionsWithRunningBalance(int clientId)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client == null)
                return NotFound("Client not found");

            var transactions = await _context.Transactions
                .Where(t => t.ClientId == clientId)
                .OrderBy(t => t.TransDate)
                .ThenBy(t => t.TransId) // Secondary sort for consistent ordering
                .ToListAsync();

            if (!transactions.Any())
                return Ok(new { Message = "No transactions found", Transactions = new List<object>() });

            // SERVER-SIDE running balance calculation
            decimal runningBalance = 0;
            var result = transactions.Select(t =>
            {
                runningBalance += (t.Added ?? 0) - (t.Subtracted ?? 0);
                return new
                {
                    transId = t.TransId,
                    clientId = t.ClientId,
                    transDate = t.TransDate,
                    added = t.Added ?? 0,
                    subtracted = t.Subtracted ?? 0,
                    agent = t.Agent ?? "Unknown",
                    runningBalance = runningBalance,
                    balance = runningBalance // Backward compatibility
                };
            }).ToList();

            return Ok(result);
        }


        // Helper methods
        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.TransId == id);
        }

       
    }
}


