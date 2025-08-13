using GemachApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GemachApp.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UpdateController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UpdateController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Update - Get all update records
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UpdateLog>>> GetUpdates()
        {
            return await _context.Updates.ToListAsync();
        }

        // GET: api/Update/{clientId} - Get specific update records for a client
        [HttpGet("{clientId}")]
        public async Task<ActionResult<IEnumerable<UpdateLog>>> GetUpdatesByClient(int clientId)
        {
            try
            {
                var updates = await _context.Updates
                    .Where(u => u.TableName == "Clients" && u.ObjectId == clientId.ToString())
                    .OrderByDescending(u => u.Timestamp)
                    .ToListAsync();

                return updates;
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving updates: {ex.Message}");
            }
        }

        // GET: api/Update/client/{clientId}
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<IEnumerable<UpdateLog>>> GetUpdatesByClientId(string clientId)
        {

            try
            {
                var updates = await _context.Updates
                    .Where(u => u.TableName == "Clients" && u.ObjectId == clientId)
                    .OrderByDescending(u => u.Timestamp)
                    .ToListAsync();

                Console.WriteLine($"Found {updates.Count} updates for Client ID: {clientId}");
                return Ok(updates);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving client updates: {ex.Message}");
            }
        }

        // GET: api/Update/agent/{agentId}     - Get updates TO a specific agent record
        [HttpGet("agent/{agentID}")]
        public async Task<ActionResult<IEnumerable<UpdateLog>>> GetUpdatesByAgentId(string agentID)
        {
            try
            {
                var updates = await _context.Updates
                    .Where(u => u.TableName == "Agent" && u.ObjectId == agentID)
                    .OrderByDescending(u => u.Timestamp)
                    .ToListAsync();

                Console.WriteLine($"Found {updates.Count} updates for Agent ID: {agentID}");
                return Ok(updates);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving agent updates: {ex.Message}");
            }
        }

        // GET: api/Update/by-agent/{agentID} - Get updates made BY a specific agent
        [HttpGet("by-agent/{agentID}")]
        public async Task<ActionResult<IEnumerable<UpdateLog>>> GetUpdatesByAgentsID(string agentID)
        {
            try
            {
                Console.WriteLine($"Fetching updates made BY Agent ID: {agentID}");

                // First, let's see what agent values are actually stored in the Updates table
                var distinctAgentValues = await _context.Updates
                    .Where(u => !string.IsNullOrEmpty(u.Agent))
                    .Select(u => u.Agent)
                    .Distinct()
                    .ToListAsync();

                Console.WriteLine($"All agent values found in Updates table: {string.Join(", ", distinctAgentValues)}");

                // Now search for updates made BY this agent
                // The Agent field in UpdateLog stores the ID of the agent who made the change
                var updates = await _context.Updates
                    .Where(u => u.Agent == agentID)  // Direct comparison with agent ID
                    .OrderByDescending(u => u.Timestamp)
                    .ToListAsync();

                Console.WriteLine($"Found {updates.Count} updates made BY Agent ID: {agentID}");

                // If no updates found, let's also check if we need to look up the agent name
                if (updates.Count == 0)
                {
                    // Try to find the agent to get their name
                    if (int.TryParse(agentID, out int agentIdInt))
                    {
                        var agent = await _context.Agents.FindAsync(agentIdInt);
                        if (agent != null)
                        {
                            Console.WriteLine($"Agent name for ID {agentID}: '{agent.AgentName}'");

                            // Try searching by agent name as well
                            var updatesByName = await _context.Updates
                                .Where(u => u.Agent == agent.AgentName)
                                .OrderByDescending(u => u.Timestamp)
                                .ToListAsync();

                            Console.WriteLine($"Found {updatesByName.Count} updates made BY Agent Name: {agent.AgentName}");

                            if (updatesByName.Count > 0)
                            {
                                updates = updatesByName;
                            }
                        }
                    }
                }

                // Debug: Show what we found
                if (updates.Any())
                {
                    Console.WriteLine($"Sample update: Agent={updates.First().Agent}, Column={updates.First().ColumName}, Table={updates.First().TableName}");
                }

                return Ok(updates);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving updates made by agent: {ex.Message}");
                return BadRequest($"Error retrieving updates made by agent: {ex.Message}");
            }
        }

        // GET: api/Update/names?firstName=John&lastName=Doe
        [HttpGet("names")]
        public async Task<ActionResult<IEnumerable<UpdateLog>>> GetUpdatesByNames([FromQuery] string firstName, [FromQuery] string lastName)
        {
            try
            {
                var client = await _context.Clients
                    .FirstOrDefaultAsync(c => c.ClientFirstName == firstName && c.ClientLastName == lastName);

                if (client == null)
                    return NotFound("Client not found.");

                var updates = await _context.Updates
                    .Where(u => u.TableName == "Clients" && u.ObjectId == client.ClientId.ToString())
                    .OrderByDescending(u => u.Timestamp)
                    .ToListAsync();

                return Ok(updates);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving updates by names: {ex.Message}");
            };
        }

        // PUT: api/Update/{id} - Update a client and log the change
        [HttpPut("{id}")]
          public async Task<IActionResult> UpdateClient(int id, [FromBody] UpdateClientRequest request)
        {
            if (id != request.ClientId)
            {
                return BadRequest("Client ID mismatch");
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.ClientFirstName))
            {
                return BadRequest("First Name is required");
            }

            if (string.IsNullOrWhiteSpace(request.Phonenumber))
            {
                return BadRequest("Phone number is required");
            }

            if (string.IsNullOrWhiteSpace(request.Agent))
            {
                return BadRequest("Agent name is required for updates");
            }

            // Validate field lengths
            if (request.ClientFirstName.Length < 3 || request.ClientFirstName.Length > 40)
            {
                return BadRequest("First Name must be between 3 and 40 characters");
            }

            if (request.Phonenumber.Length < 10 || request.Phonenumber.Length > 18)
            {
                return BadRequest("Phone number must be between 10 and 18 characters");
            }

            var existingClient = await _context.Clients.AsNoTracking().FirstOrDefaultAsync(c => c.ClientId == id);
            if (existingClient == null)
            {
                return NotFound("Client not found");
            }

            // DEBUG: Log password comparison
            Console.WriteLine($"=== PASSWORD DEBUG ===");
            Console.WriteLine($"Existing Password: '{existingClient.ClientPassword ?? "NULL"}'");
            Console.WriteLine($"New Password: '{request.ClientPassword ?? "NULL"}'");
            Console.WriteLine($"Are they equal? {(existingClient.ClientPassword ?? "") == (request.ClientPassword ?? "")}");


            // Compare fields and create update records for each changed field
            await LogFieldChanges(existingClient, request, request.Agent);

            // Prepare the client for update - preserve fields that shouldn't be changed by frontend
            var clientToUpdate = new Client
            {
                ClientId = request.ClientId,
                ClientFirstName = request.ClientFirstName,
                ClientLastName = request.ClientLastName ?? "",
                Phonenumber = request.Phonenumber,
                Email = request.Email,
                ClientPassword = !string.IsNullOrEmpty(request.ClientPassword) ? request.ClientPassword : existingClient.ClientPassword, // Only update password if provided
                ClientOpenDate = existingClient.ClientOpenDate, // Preserve original open date
                Urav = request.Urav,
                Comments = request.Comments ?? "",
                SelectedPosition = request.SelectedPosition ?? "בעל הבית",
                Agent = request.Agent
            };

            // Update the client in the database
            _context.Entry(clientToUpdate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating client: {ex.Message}");
            }

            return NoContent();
        }

        private async Task LogFieldChanges(Client existingClient, UpdateClientRequest updatedClient, string agent)
        {
            var updates = new List<UpdateLog>();
            var timestamp = DateTime.UtcNow;

            Console.WriteLine($"=== LOGGING FIELD CHANGES ===");
            Console.WriteLine($"Agent: {agent}");

            // Check each field for changes and log them individually
            if (existingClient.ClientFirstName != updatedClient.ClientFirstName)
            {
                updates.Add(new UpdateLog
                {
                    ObjectId = existingClient.ClientId.ToString(),
                    TableName = "Clients",
                  //  RecordId = existingClient.ClientId,
                    ColumName = "FirstName",
                    PrevVersion = existingClient.ClientFirstName ?? "",
                    UpdatedVersion = updatedClient.ClientFirstName ?? "",
                    Agent = agent ?? "Unknown",
                    Timestamp = timestamp
                });
            }

            if (existingClient.ClientLastName != updatedClient.ClientLastName)
            {
                updates.Add(new UpdateLog
                {
                    TableName = "Clients",
                    ObjectId = existingClient.ClientId.ToString(),
                    ColumName = "LastName",
                    PrevVersion = existingClient.ClientLastName ?? "",
                    UpdatedVersion = updatedClient.ClientLastName ?? "",
                    Agent = agent ?? "Unknown",
                    Timestamp = timestamp
                });
            }

            if (existingClient.Phonenumber != updatedClient.Phonenumber)
            {
                updates.Add(new UpdateLog
                {
                    TableName = "Clients",
                    ObjectId = existingClient.ClientId.ToString(),
                    ColumName = "PhoneNumber",
                    PrevVersion = existingClient.Phonenumber ?? "",
                    UpdatedVersion = updatedClient.Phonenumber ?? "",
                    Agent = agent ?? "Unknown",
                    Timestamp = timestamp
                });
            }

            if (existingClient.UpdateByEmail != updatedClient.UpdateByEmail)
            {
                updates.Add(new UpdateLog
                {
                    TableName = "Clients",
                    ObjectId = existingClient.ClientId.ToString(),
                    ColumName = "UpdateByEmail",
                    PrevVersion = existingClient.UpdateByEmail.ToString(),
                    UpdatedVersion = updatedClient.UpdateByEmail.ToString(),
                    Agent = agent ?? "Unknown",
                    Timestamp = timestamp
                });
            }

            // Password change detection with better debugging
            var existingPassword = existingClient.ClientPassword ?? "";
            var newPassword = updatedClient.ClientPassword ?? "";

            Console.WriteLine($"=== PASSWORD CHANGE CHECK ===");
            Console.WriteLine($"Existing Password: '{existingPassword}' (Length: {existingPassword.Length})");
            Console.WriteLine($"New Password: '{newPassword}' (Length: {newPassword.Length})");
            Console.WriteLine($"Is new password empty? {string.IsNullOrEmpty(newPassword)}");
            Console.WriteLine($"Are passwords different? {existingPassword != newPassword}");

            // Only log password changes when a new password is actually provided and it's different
            if (!string.IsNullOrEmpty(newPassword) && existingPassword != newPassword)
            {
                Console.WriteLine("PASSWORD CHANGE DETECTED - Adding to updates");
                updates.Add(new UpdateLog
                {
                    TableName = "Clients",
                    ObjectId = existingClient.ClientId.ToString(),
                    ColumName = "ClientPassword",
                    PrevVersion = "***", // Don't log actual passwords for security
                    UpdatedVersion = "***", // Don't log actual passwords for security
                    Agent = agent ?? "Unknown",
                    Timestamp = timestamp
                });
            }
            else
            {
                Console.WriteLine("NO PASSWORD CHANGE DETECTED");
            }

            if (existingClient.Comments != updatedClient.Comments)
            {
                updates.Add(new UpdateLog
                {
                    TableName = "Clients",
                    ObjectId = existingClient.ClientId.ToString(),
                    ColumName = "Comments",
                    PrevVersion = existingClient.Comments ?? "",
                    UpdatedVersion = updatedClient.Comments ?? "",
                    Agent = agent ?? "Unknown",
                    Timestamp = timestamp
                });
            }

            if (existingClient.SelectedPosition != updatedClient.SelectedPosition)
            {
                updates.Add(new UpdateLog
                {
                    TableName = "Clients",
                    ObjectId = existingClient.ClientId.ToString(),
                    ColumName = "SelectedPosition",
                    PrevVersion = existingClient.SelectedPosition ?? "",
                    UpdatedVersion = updatedClient.SelectedPosition ?? "",
                    Agent = agent ?? "Unknown",
                    Timestamp = timestamp
                });
            }

            if (existingClient.Urav != updatedClient.Urav)
            {
                updates.Add(new UpdateLog
                {
                    TableName = "Clients",
                    ObjectId = existingClient.ClientId.ToString(),
                    ColumName = "Urav",
                    PrevVersion = existingClient.Urav.ToString(),
                    UpdatedVersion = updatedClient.Urav.ToString(),
                    Agent = agent ?? "Unknown",
                    Timestamp = timestamp
                });
            }

            // Add all update records to the context
            if (updates.Any())
            {
                _context.Updates.AddRange(updates);
                await _context.SaveChangesAsync();
            }
        }


        // DELETE: api/Update/{id} - Delete an update record
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUpdate(int id)
        {
            var update = await _context.Updates.FindAsync(id);
            if (update == null)
            {
                return NotFound();
            }

            _context.Updates.Remove(update);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.ClientId == id);
        }

        private async Task LogAgentFieldChanges(Agent existingAgent, Agent updatedAgent, string agentMakingChange)
        {
            var updates = new List<UpdateLog>();
            var timestamp = DateTime.UtcNow;

            Console.WriteLine($"=== LOGGING AGENT FIELD CHANGES ===");
            Console.WriteLine($"Agent making change: {agentMakingChange}");
            Console.WriteLine($"Existing agent: ID={existingAgent.Id}, Name='{existingAgent.AgentName}'");
            Console.WriteLine($"Updated agent: ID={updatedAgent.Id}, Name='{updatedAgent.AgentName}'");

            // Check each field for changes and log them individually
            if (existingAgent.AgentName != updatedAgent.AgentName)
            {
                Console.WriteLine($"Agent name changed: '{existingAgent.AgentName}' -> '{updatedAgent.AgentName}'");
                updates.Add(new UpdateLog
                {
                    ObjectId = existingAgent.Id.ToString(),
                    TableName = "Agent",
                    ColumName = "AgentName", // Note: you might have ColumName vs ColumnName inconsistency
                    PrevVersion = existingAgent.AgentName ?? "",
                    UpdatedVersion = updatedAgent.AgentName ?? "",
                    Agent = agentMakingChange ?? "Unknown",
                    Timestamp = timestamp
                });
            }

            if (existingAgent.AgentPassword != updatedAgent.AgentPassword)
            {
                Console.WriteLine("Agent password changed");
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
                Console.WriteLine($"Adding {updates.Count} update records to database");
                _context.Updates.AddRange(updates);
                await _context.SaveChangesAsync();
                Console.WriteLine("Agent update records saved successfully");
            }
            else
            {
                Console.WriteLine("No agent field changes detected");
            }
        }

        public class UpdateClientRequest
        {
            public int ClientId { get; set; }
            public string ClientFirstName { get; set; }
            public string ClientLastName { get; set; }
            public string Phonenumber { get; set; }
            public string Email { get; set; }
            public string Comments { get; set; }
            public string SelectedPosition { get; set; }
            public bool Urav { get; set; }
            public bool UpdateByEmail { get; set; }
            public string ClientPassword { get; set; }
            public string Agent { get; set; }
        }

        public class UpdateAgentRequest
        {
            public int Id { get; set; }
            public string AgentName { get; set; }
            public string AgentPassword { get; set; }
            public string AgentMakingChange { get; set; } // Who is making this change
        }

    }
}