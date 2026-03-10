

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GemachApp.Data;
using System.Globalization;

namespace GemachApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChecksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ChecksController> _logger;

        public ChecksController(AppDbContext context, ILogger<ChecksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // CREATE CHECK
        [HttpPost]
        public async Task<IActionResult> CreateCheck([FromBody] Check check)
        {
            try
            {
                if (check == null)
                    return BadRequest("Invalid payload");

                if (check.Sum <= 0)
                    return BadRequest("Sum must be greater than zero");

                if (await _context.Checks.AnyAsync(c => c.CheckId == check.CheckId))
                    return BadRequest($"Check number {check.CheckId} already exists");

                check.CheckIssuedDate = DateTime.UtcNow;

                _context.Checks.Add(check);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Check created successfully",
                    checkId = check.CheckId,
                    issuedDate = check.CheckIssuedDate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateCheck failed");
                return StatusCode(500, "Internal server error");
            }
        }

        // ALL CHECKS
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _context.Checks.ToListAsync());
        }

        // 🔑 DATE SEARCH (FIXED)
        [HttpGet("history/date/{date}")]
        public async Task<IActionResult> GetByDate(string date)
        {
            try
            {
                if (!DateOnly.TryParseExact(
                        date,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var parsedDate))
                {
                    return BadRequest("Invalid date format. Use yyyy-MM-dd");
                }

                var results = await _context.Checks
                    .Where(c => DateOnly.FromDateTime(c.CheckIssuedDate) == parsedDate)
                    .OrderByDescending(c => c.CheckIssuedDate)
                    .Select(c => new
                    {
                        c.CheckId,
                        c.ClientId,
                        c.ClientName,
                        c.OrderTo,
                        Amount = c.Sum,
                        c.AgentName,
                        c.AgentId,
                        c.TransId,
                        DateIssued = c.CheckIssuedDate
                    })
                    .ToListAsync();

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetByDate failed");
                return StatusCode(500, "Failed fetching checks by date");
            }
        }

        // CHECK NUMBER
        [HttpGet("history/check/{checkNumber:int}")]
        public async Task<IActionResult> GetByCheckNumber(int checkNumber)
        {
            var data = await _context.Checks
                .Where(c => c.CheckId == checkNumber)
                .ToListAsync();

            return Ok(data);
        }

        // CLIENT
        [HttpGet("history/client/{clientId:int}")]
        public async Task<IActionResult> GetByClient(int clientId)
        {
            var data = await _context.Checks
                .Where(c => c.ClientId == clientId)
                .OrderByDescending(c => c.CheckIssuedDate)
                .ToListAsync();

            return Ok(data);
        }
    }
}