

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GemachApp.Data;
//using GemachApp.Models;

namespace GemachApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransactionsController(AppDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // PROCESS TRANSACTION
        // =========================================================
        [HttpPost("ProcessTransaction")]
        public async Task<IActionResult> ProcessTransaction([FromBody] TransactionRequest request)
        {
            if (request == null)
                return BadRequest("Missing transaction payload");

            if ((request.Added ?? 0) == 0 && (request.Subtracted ?? 0) == 0)
                return BadRequest("Either Added or Subtracted must be greater than zero");

            IActionResult result = StatusCode(500, "Unhandled error");

            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _context.Database.BeginTransactionAsync();

                try
                {
                    // 1️⃣ Validate client
                    var client = await _context.Clients.FindAsync(request.ClientId);
                    if (client == null)
                    {
                        result = NotFound("Client not found");
                        return;
                    }

                    // 2️⃣ Get or create account
                    var account = await _context.Accounts
                        .FirstOrDefaultAsync(a => a.ClientId == request.ClientId);

                    if (account == null)
                    {
                        account = new Account
                        {
                            ClientId = request.ClientId,
                            TotalAmount = 0m,
                            UpdateBalDate = DateTime.UtcNow
                        };

                        _context.Accounts.Add(account);
                        await _context.SaveChangesAsync();
                    }

                    // 3️⃣ Calculate new balance safely
                    decimal add = request.Added ?? 0m;
                    decimal sub = request.Subtracted ?? 0m;
                    decimal currentBalance = account.TotalAmount ?? 0m;
                    decimal newBalance = currentBalance + add - sub;

                    if (newBalance < 0)
                    {
                        result = BadRequest("Transaction would result in negative balance");
                        return;
                    }

                    // 4️⃣ Calculate running totals
                    var totals = await _context.Transactions
                        .Where(t => t.ClientId == request.ClientId)
                        .Select(t => new
                        {
                            Added = t.Added ?? 0m,
                            Subtracted = t.Subtracted ?? 0m
                        })
                        .ToListAsync();

                    decimal totalAdded = totals.Sum(t => t.Added) + add;
                    decimal totalSubtracted = totals.Sum(t => t.Subtracted) + sub;

                    // 5️⃣ Create transaction
                    var transaction = new Transaction
                    {
                        ClientId = request.ClientId,
                        Added = add > 0 ? add : null,
                        Subtracted = sub > 0 ? sub : null,
                        TotalAdded = totalAdded,
                        TotalSubtracted = totalSubtracted,
                        Agent = request.Agent ?? "System",
                        TransDate = DateTime.UtcNow
                    };

                    _context.Transactions.Add(transaction);

                    // 6️⃣ Update account
                    account.TotalAmount = newBalance;
                    account.UpdateBalDate = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();

                    result = Ok(new
                    {
                        message = "Transaction processed successfully",
                        transactionId = transaction.TransId,
                        balance = newBalance
                    });
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    Console.Error.WriteLine(ex);
                    result = StatusCode(500, "Transaction failed");
                }
            });

            return result;
        }

        // =========================================================
        // GET TRANSACTIONS BY CLIENT
        // =========================================================
        [HttpGet("GetTransactionsByClient/{clientId}")]
        public async Task<IActionResult> GetTransactionsByClient(int clientId)
        {
            var transactions = await _context.Transactions
                .Where(t => t.ClientId == clientId)
                .OrderByDescending(t => t.TransDate)
                .ToListAsync();

            return Ok(transactions);
        }

        // =========================================================
        // DELETE TRANSACTION
        // =========================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            IActionResult result = StatusCode(500, "Unhandled error");

            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await _context.Database.BeginTransactionAsync();

                try
                {
                    var transaction = await _context.Transactions.FindAsync(id);
                    if (transaction == null)
                    {
                        result = NotFound("Transaction not found");
                        return;
                    }

                    var account = await _context.Accounts
                        .FirstOrDefaultAsync(a => a.ClientId == transaction.ClientId);

                    if (account == null)
                    {
                        result = NotFound("Account not found");
                        return;
                    }

                    decimal add = transaction.Added ?? 0m;
                    decimal sub = transaction.Subtracted ?? 0m;

                    account.TotalAmount = (account.TotalAmount ?? 0m) - add + sub;
                    account.UpdateBalDate = DateTime.UtcNow;

                    _context.Transactions.Remove(transaction);
                    await _context.SaveChangesAsync();
                    await tx.CommitAsync();

                    result = Ok("Transaction deleted");
                }
                catch (Exception ex)
                {
                    await tx.RollbackAsync();
                    Console.Error.WriteLine(ex);
                    result = StatusCode(500, "Delete failed");
                }
            });

            return result;
        }

        public class TransactionRequest
        {
            public int ClientId { get; set; }
            public decimal? Added { get; set; }
            public decimal? Subtracted { get; set; }
            public string? Agent { get; set; }
        }


    }
}






/*


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

        public TransactionsController(AppDbContext context, IEmailService? emailService)
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
                transaction.TransDate = DateTime.UtcNow;

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
                    account.UpdateBalDate = DateTime.UtcNow;

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
            try
            {
                Console.WriteLine($"[GetTransactionsByClient] Starting for clientId: {clientId}");

                // Validate client exists first
                var clientExists = await _context.Clients.AnyAsync(c => c.ClientId == clientId);
                if (!clientExists)
                {
                    Console.WriteLine($"[GetTransactionsByClient] Client {clientId} not found");
                    return NotFound(new { message = "Client not found" });
                }

                Console.WriteLine($"[GetTransactionsByClient] Client {clientId} exists, fetching transactions...");

                var transactions = await _context.Transactions
                    .Where(t => t.ClientId == clientId)
                    .OrderBy(t => t.TransDate)
                    .ThenBy(t => t.TransId)
                    .ToListAsync();

                Console.WriteLine($"[GetTransactionsByClient] Found {transactions.Count} transactions");

                if (!transactions.Any())
                {
                    Console.WriteLine($"[GetTransactionsByClient] No transactions found for client {clientId}");
                    return Ok(new List<object>());
                }

                // Calculate running balance server-side
                decimal runningBalance = 0;
                var result = new List<object>();

                foreach (var t in transactions)
                {
                    runningBalance += (t.Added ?? 0) - (t.Subtracted ?? 0);

                    result.Add(new
                    {
                        transId = t.TransId,
                        clientId = t.ClientId,
                        transDate = t.TransDate,
                        added = t.Added ?? 0,
                        subtracted = t.Subtracted ?? 0,
                        agent = t.Agent ?? "Unknown",
                        runningBalance = runningBalance
                    });
                }

                Console.WriteLine($"[GetTransactionsByClient] Successfully processed {result.Count} transactions");
                Console.WriteLine($"[GetTransactionsByClient] Final running balance: {runningBalance}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[GetTransactionsByClient] ERROR for clientId {clientId}:");
                Console.Error.WriteLine($"Message: {ex.Message}");
                Console.Error.WriteLine($"Stack Trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.Error.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    Console.Error.WriteLine($"Inner Stack Trace: {ex.InnerException.StackTrace}");
                }

                return StatusCode(500, new
                {
                    message = "An error occurred while fetching transactions",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }
     


        // POST: api/Transactions/ProcessTransaction
        [HttpPost("ProcessTransaction")]

        public async Task<IActionResult> ProcessTransaction([FromBody] TransactionRequest request)
        {
            if (request == null)
                return BadRequest("Transaction data is missing");

            if (string.IsNullOrWhiteSpace(request.Agent))
            {
                request.Agent = User?.Identity?.Name ?? "System";
            }

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
                Console.WriteLine($"Processing transaction for ClientId: {request.ClientId}");

                // 1. Validate client exists
                var client = await _context.Clients.FindAsync(request.ClientId);
                if (client == null)
                {
                    Console.WriteLine($"Client {request.ClientId} not found");
                    await transaction.RollbackAsync();
                    return NotFound("Client not found");
                }

                Console.WriteLine($"Client found: {client.ClientFirstName} {client.ClientLastName}");

                // Process the amounts
                decimal addAmount = request.Added ?? 0;
                decimal subtractAmount = request.Subtracted ?? 0;

                Console.WriteLine($"Processing: Added={addAmount}, Subtracted={subtractAmount}");

                // 2. Get or create account
                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.ClientId == request.ClientId);

                if (account == null)
                {
                    Console.WriteLine("Creating new account");
                    account = new Account
                    {
                        ClientId = request.ClientId,
                        TotalAmount = 0,
                        UpdateBalDate = DateTime.UtcNow
                    };
                    _context.Accounts.Add(account);
                    await _context.SaveChangesAsync(); // Save to get the AccountId
                }

                Console.WriteLine($"Current account balance: {account.TotalAmount}");

                // 3. Check for negative balance (business rule)
                decimal currentBalance = account.TotalAmount ?? 0;
                decimal newBalance = currentBalance + addAmount - subtractAmount;

                Console.WriteLine($"New balance would be: {newBalance}");

                if (newBalance < 0)
                {
                    Console.WriteLine("Transaction rejected: would result in negative balance");
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

                Console.WriteLine($"Cumulative totals - Added: {totalAdded}, Subtracted: {totalSubtracted}");

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

                Console.WriteLine("Saving transaction to database...");
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                Console.WriteLine($"Transaction saved successfully with ID: {newTransaction.TransId}");

                // 7. Send email if requested AND email service is available
                if (request.SendEmail == true && !string.IsNullOrEmpty(client.Email) && _emailService != null)
                {
                    try
                    {
                        Console.WriteLine($"Sending email to {client.Email}");
                        await _emailService.SendEmailAsync(
                            client.Email,
                            "Transaction Confirmation",
                            $"Dear {client.ClientFirstName},\n\n" +
                            $"Your transaction has been processed:\n" +
                            $"Added: {(addAmount > 0 ? $"${addAmount:F2}" : "N/A")}\n" +
                            $"Subtracted: {(subtractAmount > 0 ? $"${subtractAmount:F2}" : "N/A")}\n" +
                            $"New Balance: ${account.TotalAmount:F2}\n\n" +
                            $"Transaction processed by: {request.Agent}\n" +
                            $"Date: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}"
                        );
                        Console.WriteLine("Email sent successfully");
                    }
                    catch (Exception emailEx)
                    {
                        // Log email error but don't fail the transaction
                        Console.WriteLine($"Email sending failed: {emailEx.Message}");
                    }
                }
                else if (request.SendEmail == true && _emailService == null)
                {
                    Console.WriteLine("Email requested but email service is not configured");
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
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.Error.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                return StatusCode(500, new
                {
                    message = "An error occurred while processing the transaction",
                    error = ex.Message,
                    details = ex.InnerException?.Message
                });
            }
        }
       
      //* ----->  [HttpGet("TestConnection")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var count = await _context.Transactions.CountAsync();
                return Ok(new
                {
                    message = "Connection successful",
                    transactionCount = count,
                    databaseName = _context.Database.GetDbConnection().Database
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
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

*/
