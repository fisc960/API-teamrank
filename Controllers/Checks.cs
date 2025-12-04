

using Microsoft.AspNetCore.Mvc;
using GemachApp.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GemachApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChecksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChecksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCheck([FromBody] Check check)
        {
            try
            {
                Console.WriteLine($"Creating check with CheckId: {check.CheckId}");

                // Validate that CheckId doesn't already exist
                var existingCheck = await _context.Checks.FindAsync(check.CheckId);
                if (existingCheck != null)
                {
                    return BadRequest(new { message = $"Check number {check.CheckId} already exists" });
                }

                // Validate Sum
                if (check.Sum <= 0)
                {
                    return BadRequest(new { message = "Sum must be greater than 0." });
                }

                // Set the issued date
                check.CheckIssuedDate = DateTime.UtcNow;

                _context.Checks.Add(check);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Check created successfully with ID: {check.CheckId}");

                // Return the check with the CheckId
                return Ok(new
                {
                    message = "Check created successfully",
                    checkId = check.CheckId,
                    checkIssuedDate = check.CheckIssuedDate
                });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error creating check: {ex.Message}");
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");

                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message ?? "No inner exception",
                    stack = ex.StackTrace
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllChecks()
        {
            try
            {
                var checks = await _context.Checks.ToListAsync();
                return Ok(checks);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting all checks: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /*
        [HttpGet("history/client/{clientId}")]
        public async Task<IActionResult> GetCheckById(int id)
        {
            try
            {
                var check = await _context.Checks.FindAsync(id);
                if (check == null)
                    return NotFound(new { message = $"Check {id} not found" });

                return Ok(check);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting check by ID: {ex.Message}");
                return StatusCode(500, new { message = ex.Message });
            }
        }*/

        [HttpGet("history/date/{date}")]
        public async Task<IActionResult> GetChecksByDate(string date)
        {
            try
            {
                Console.WriteLine($"Received date parameter: {date}");

                // Parse the date string
                if (!DateTime.TryParse(date, out DateTime parsedDate))
                {
                    return BadRequest(new { message = $"Invalid date format: {date}. Use YYYY-MM-DD" });
                }

                Console.WriteLine($"Parsed date: {parsedDate:yyyy-MM-dd}");

                var data = await _context.Checks
                    .Where(c => c.CheckIssuedDate.Date == parsedDate.Date)
                    .OrderByDescending(c => c.CheckIssuedDate)
                    .Select(c => new
                    {
                        checkId = c.CheckId,
                        checkNumber = c.CheckId,
                        clientId = c.ClientId,
                        clientName = c.ClientName,
                        dateIssued = c.CheckIssuedDate,
                        amount = c.Sum,
                        orderTo = c.OrderTo,
                        agent = c.AgentName,
                        agentId = c.AgentId,
                        transId = c.TransId
                    })
                    .ToListAsync();

                Console.WriteLine($"Found {data.Count} checks for date {parsedDate:yyyy-MM-dd}");

                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting checks by date: {ex.Message}");
                Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.Error.WriteLine($"Inner exception: {ex.InnerException?.Message}");

                return StatusCode(500, new
                {
                    message = "Error fetching checks by date",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("history/check/{checkNumber}")]
        public async Task<IActionResult> GetChecksByNumber(int checkNumber)
        {
            try
            {
                Console.WriteLine($"Searching for check number: {checkNumber}");

                var data = await _context.Checks
                    .Where(c => c.CheckId == checkNumber)
                    .Select(c => new
                    {
                        checkId = c.CheckId,
                        checkNumber = c.CheckId,
                        clientId = c.ClientId,
                        clientName = c.ClientName,
                        dateIssued = c.CheckIssuedDate,
                        amount = c.Sum,
                        orderTo = c.OrderTo,
                        agent = c.AgentName,
                        agentId = c.AgentId,
                        transId = c.TransId
                    })
                    .ToListAsync();

                Console.WriteLine($"Found {data.Count} checks with number {checkNumber}");

                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting check by number: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error fetching check by number",
                    error = ex.Message
                });
            }
        }

        [HttpGet("history/client/{clientId}")]
        public async Task<IActionResult> GetChecksByClient(int clientId)
        {
            try
            {
                Console.WriteLine($"Searching for checks for client: {clientId}");

                var data = await _context.Checks
                    .Where(c => c.ClientId == clientId)
                    .OrderByDescending(c => c.CheckIssuedDate)
                    .Select(c => new
                    {
                        checkId = c.CheckId,
                        checkNumber = c.CheckId,
                        clientId = c.ClientId,
                        clientName = c.ClientName,
                        dateIssued = c.CheckIssuedDate,
                        amount = c.Sum,
                        orderTo = c.OrderTo,
                        agent = c.AgentName,
                        agentId = c.AgentId,
                        transId = c.TransId
                    })
                    .ToListAsync();

                Console.WriteLine($"Found {data.Count} checks for client {clientId}");

                return Ok(data);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error getting checks by client: {ex.Message}");
                return StatusCode(500, new
                {
                    message = "Error fetching checks by client",
                    error = ex.Message
                });
            }
        }
    }
}




/*

using Microsoft.AspNetCore.Mvc;
using GemachApp.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GemachApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChecksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ChecksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCheck([FromBody] Check check)
        {
            /*try
             {
                 // Don't set CheckId - let SQL Server auto-generate it
                 var newCheck = new Check
                 {
                     // Remove CheckId from here - it will be auto-generated
                     ClientName = check.ClientName,
                     ClientId = check.ClientId,
                     OrderTo = check.OrderTo,
                     Sum = check.Sum,
                     TransId = check.TransId,
                     AgentName = check.AgentName,
                     AgentId = check.AgentId,
                     CheckIssuedDate = DateTime.Now // or DateTime.UtcNow
                 };*//*
try
            {
                // Validate that CheckId doesn't already exist
                var existingCheck = await _context.Checks.FindAsync(check.CheckId);

                if (existingCheck != null)
                {
                    return BadRequest(new { message = $"Check number {check.CheckId} already exists" });
                }

                // Validate Sum
                if (check.Sum <= 0)
                {
                    return BadRequest(new { message = "Sum must be greater than 0." });
                }

                // Set the issued date
                check.CheckIssuedDate = DateTime.UtcNow;


                _context.Checks.Add(check);
                await _context.SaveChangesAsync();

                // Return the check with the auto-generated CheckId
                return Ok(new
                {
                    message = "Check created successfully",
                    checkId = check.CheckId,
                    checkIssuedDate = check.CheckIssuedDate
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message,
                    inner = ex.InnerException?.Message ?? "No inner exception",
                     stack = ex.StackTrace
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllChecks()
        {
            var checks = await _context.Checks.ToListAsync();
            return Ok(checks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCheckById(int id)
        {
            var check = await _context.Checks.FindAsync(id);
            if (check == null)
                return NotFound();

            return Ok(check);
        }

        [HttpGet("history/date/{date}")]
        public async Task<IActionResult> GetChecksByDate(DateTime date)
        {
            var data = await _context.Checks
                .Where(c => c.CheckIssuedDate.Date == date.Date)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("history/check/{checkNumber}")]
        public async Task<IActionResult> GetChecksByNumber(int checkNumber)
        {
            var data = await _context.Checks
                .Where(c => c.CheckId == checkNumber)
                .ToListAsync();

            return Ok(data);
        }

        [HttpGet("history/client/{clientId}")]
        public async Task<IActionResult> GetChecksByClient(int clientId)
        {
            var data = await _context.Checks
                .Where(c => c.ClientId == clientId)
                .ToListAsync();

            return Ok(data);
        }



    }
}*/