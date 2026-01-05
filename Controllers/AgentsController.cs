//using Azure.Core;
using GemachApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GemachApp.Controllers.UpdateController;


namespace GemachApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentsController : ControllerBase
    {
        private readonly AppDbContext _context;
       

        public AgentsController(AppDbContext context)
        {
            _context = context;
          
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] NewAgent dto)
        {
            try
            {

                // Debug logging
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
                AgentPassword = dto.Password, //  In production, hash passwords!
                AgentOpenDate = DateTime.UtcNow,
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
                
                // Return detailed error for debugging (remove in production)
                return StatusCode(500, new { 
                    message = "Internal server error", 
                    error = ex.Message,
                    stackTrace = ex.StackTrace
    });
            }

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


        // GET: api/agent
        [HttpGet]
        public async Task<IActionResult> GetAgents()
        {
            try
            {
                var agents = await _context.Agents.ToListAsync();
            // Convert to AgentResponse to return agentname/agentpassword
            var agentResponses = agents.Select(a => new AgentResponse
            {
                Id = a.Id,
                AgentName = a.AgentName,
                AgentPassword = a.AgentPassword
            }).ToList();

            return Ok(agentResponses);
        }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetAgents: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        

        // DELETE: api/agent/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAgent(int id)
        {
            try
            {
                var agent = await _context.Agents.FindAsync(id);
            if (agent == null)
            {
                return NotFound(new { message = "Agent not found" });
            }


            _context.Agents.Remove(agent);

            _context.Updates.Add(new UpdateLog
            {
                TableName = "Agent",
                ObjectId = agent.Id.ToString(),
               // ObjectId = existingAgent.Id.ToString(),
                ColumName = "Deleted",
                PrevVersion = $"Name: {agent.AgentName}, Password: {agent.AgentPassword}",
                UpdatedVersion = "DELETED",
                Agent = "ADMIN",
                Timestamp = DateTime.UtcNow
            });


                await _context.SaveChangesAsync();
                return Ok(new { message = "Agent deleted successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in DeleteAgent: {ex.Message}");
                return BadRequest(new { message = "Failed to delete agent", error = ex.Message });
            }
        }

        private void LogAgentFieldChanges(Agent existingAgent, Agent updatedAgent, string agentMakingChange)
        {
            var updates = new List<UpdateLog>();
            var timestamp = DateTime.UtcNow;

            // Check each field for changes and log them individually
            if (existingAgent.AgentName != updatedAgent.AgentName)
            {
                updates.Add(new UpdateLog
                {
                    ObjectId = existingAgent.Id.ToString(),
                    TableName = "Agent",
                    ColumName = "AgentName",
                    PrevVersion = existingAgent.AgentName ?? "",
                    UpdatedVersion = updatedAgent.AgentName ?? "",
                    Agent = agentMakingChange ?? "Unknown",
                    Timestamp = timestamp
                });
            }

            if (existingAgent.AgentPassword != updatedAgent.AgentPassword)
            {
                updates.Add(new UpdateLog
                {
                    ObjectId = existingAgent.Id.ToString(),
                    TableName = "Agent",
                    ColumName = "AgentPassword",
                    PrevVersion = "***", // Don't log actual passwords for security
                    UpdatedVersion = "***", // Don't log actual passwords for security
                    Agent = agentMakingChange ?? "Unknown",
                    Timestamp = timestamp
                });
            }

            // Add all update records to the context
            if (updates.Any())
            {
                _context.Updates.AddRange(updates);
               // await _context.SaveChangesAsync();
            }
        }

        private bool AgentExists(int id)
        {
            return _context.Agents.Any(e => e.Id == id);
        }

        public class UpdateAgentRequest
        {
            public int Id { get; set; }
            public string AgentName { get; set; }
            public string AgentPassword { get; set; }
            public string AgentMakingChange { get; set; } // Who is making this change
        }
    }

    // DTO for Agent signup/login (maps to your frontend)
    public class NewAgent
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }

    // DTO for Agent responses (returns agentname/agentpassword to avoid confusion with clients)
    public class AgentResponse
    {
        public int Id { get; set; }
        public string AgentName { get; set; }
        public string AgentPassword { get; set; }
    }

}
