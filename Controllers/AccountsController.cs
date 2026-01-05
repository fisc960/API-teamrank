
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GemachApp.Data;
using GemachApp.Services;


namespace GemachApp.Controllers
{
    public class AccountsController : Controller
    {
        private readonly AppDbContext _context;

        public AccountsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Accounts
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Accounts
                .Include(a => a.Client)
                .OrderBy(a => a.Client.ClientLastName)
                .ThenBy(a => a.Client.ClientFirstName);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Accounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var account = await _context.Accounts
                .Include(a => a.Client)
                .FirstOrDefaultAsync(m => m.AccountId == id);

            if (account == null)
                return NotFound();

            var recentTransactions = await _context.Transactions
                .Where(t => t.ClientId == account.ClientId)
                .OrderByDescending(t => t.TransDate)
                .Take(10)
                .ToListAsync();

            ViewData["RecentTransactions"] = recentTransactions;
            return View(account);
        }

        // GET: Accounts/Create
        public IActionResult Create()
        {
            ViewData["ClientId"] = new SelectList(_context.Clients.Select(c => new {
                c.ClientId,
                FullName = c.ClientFirstName + " " + c.ClientLastName
            }), "ClientId", "FullName");
            return View();
        }

        // POST: Accounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountId,ClientId,TotalAmount,UpdateBalDate")] Account account)
        {
            if (ModelState.IsValid)
            {
                account.UpdateBalDate = DateTime.UtcNow;
                _context.Add(account);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ClientId"] = new SelectList(_context.Clients.Select(c => new {
                c.ClientId,
                FullName = c.ClientFirstName + " " + c.ClientLastName
            }), "ClientId", "FullName", account.ClientId);
            return View(account);
        }

        // GET: Accounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
                return NotFound();

            ViewData["ClientId"] = new SelectList(_context.Clients.Select(c => new {
                c.ClientId,
                FullName = c.ClientFirstName + " " + c.ClientLastName
            }), "ClientId", "FullName", account.ClientId);
            return View(account);
        }

        // POST: Accounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AccountId,ClientId,TotalAmount,UpdateBalDate")] Account account)
        {
            if (id != account.AccountId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    account.UpdateBalDate = DateTime.UtcNow;
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Accounts.Any(e => e.AccountId == account.AccountId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ClientId"] = new SelectList(_context.Clients.Select(c => new {
                c.ClientId,
                FullName = c.ClientFirstName + " " + c.ClientLastName
            }), "ClientId", "FullName", account.ClientId);
            return View(account);
        }

        // GET: Accounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var account = await _context.Accounts
                .Include(a => a.Client)
                .FirstOrDefaultAsync(m => m.AccountId == id);

            if (account == null)
                return NotFound();

            return View(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account != null)
                _context.Accounts.Remove(account);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}

