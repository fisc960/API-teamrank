using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GemachApp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GemachApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] NewAgent loginDto)
        {
            try
            {
                if (loginDto == null)
                {
                    return BadRequest(new { message = "Request body is null" });
                }

                var agent = _context.Agents
                    .FirstOrDefault(a => a.AgentName == loginDto.Name && a.AgentPassword == loginDto.Password);

                if (agent == null)
                {
                    return Unauthorized(new { message = "Invalid name or password" });
                }

                return Ok(new { message = "Login successful", agentName = agent.AgentName, agentId = agent.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Login: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] NewAgent dto)
        {
            try
            {
                Console.WriteLine($"Received signup request - Name: '{dto?.Name}', Password: '{dto?.Password}'");

                if (dto == null)
                {
                    return BadRequest(new { message = "Request body is null" });
                }

                if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Password))
                {
                    return BadRequest(new { message = "Name and Password are required" });
                }

                var agent = new Agent
                {
                    AgentName = dto.Name,
                    AgentPassword = dto.Password, // In production, hash passwords!
                    AgentOpenDate = DateTime.Now,
                };

                Console.WriteLine($"About to add agent to context: {agent.AgentName}");
                _context.Agents.Add(agent);

                Console.WriteLine("About to save changes...");
                await _context.SaveChangesAsync();

                Console.WriteLine("Agent saved successfully!");
                return Ok(new { message = "Agent registered successfully!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Signup: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return StatusCode(500, new
                {
                    message = "Internal server error",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }


        // GET: api/Admins
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Admin>>> GetAdmin()
        {
            return await _context.Admins.ToListAsync();
        }

        // GET: api/Admins/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Admin>> GetAdmin(int id)
        {
            var admin = await _context.Admins.FindAsync(id);

            if (admin == null)
            {
                return NotFound();
            }

            return admin;
        }

        // PUT: api/Admins/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAdmin(int id, Admin admin)
        {
            if (id != admin.Id)
            {
                return BadRequest();
            }

            _context.Entry(admin).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdminExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Admins
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Admin>> PostAdmin(Admin admin)
        {
            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAdmin", new { id = admin.Id }, admin);
        }

        // DELETE: api/Admins/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }

            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AdminExists(int id)
        {
            return _context.Admins.Any(e => e.Id == id);
        }
        /*
        public class NewAgent
        {
            public string Name { get; set; }
            public string Password { get; set; }
        }*/
    }
}
