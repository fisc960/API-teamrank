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
                 };*/
            try
            {
                // Validate that CheckId doesn't already exist
                var existingCheck = await _context.Checks.FindAsync(check.CheckId);

                if (existingCheck != null)
                {
                    return BadRequest(new { message = $"Check number {check.CheckId} already exists" });
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
    }
}