
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GemachApp.Data;
using System.Net;
using GemachApp.DTOs;


namespace GemachApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Clients1Controller : ControllerBase
    {
        private readonly AppDbContext _context;

        public Clients1Controller(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Clients1
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            return await _context.Clients.ToListAsync();
        }

        
        // GET: api/Clients1/5
        [HttpGet("{ClientId}")]
        public async Task<ActionResult<object>> GetClient(int ClientId)
        {
            var client = await _context.Clients
    .Where(c => c.ClientId == ClientId)
    .Select(c => new
    {
        c.ClientId,
        c.ClientFirstName,
        c.ClientLastName,
        c.Phonenumber,
        c.Email,
        Balance = _context.Accounts
                    .Where(a => a.ClientId == ClientId)
                    .Select(a => a.TotalAmount)
                    .FirstOrDefault() ?? 0  // Default to 0 if no account exists
    })
    .FirstOrDefaultAsync();

            if (client == null)
            {
                return NotFound(new { message = $"Client with ID {ClientId} not found." });
            }

            return client;
        }
        
        // GET: api/Clients1/password/{password}
        [HttpGet("password/{password}")]
        public async Task<ActionResult<object>> GetClientByUsername(string password)
        {
            var client = await _context.Clients
                .Where(c => c.ClientPassword == password)
                .Select(c => new
                {
                    c.ClientId,
                    c.ClientFirstName,
                    c.ClientLastName,
                    c.Phonenumber,
                    c.Email,
                    Balance = _context.Accounts
                        .Where(a => a.ClientId == c.ClientId)
                        .Select(a => a.TotalAmount)
                        .FirstOrDefault() ?? 0
                })
                .FirstOrDefaultAsync();

            if (client == null)
            {
                return NotFound(new { message = $"Client with password {password} not found." });
            }

            return client;
        }


        // PUT: api/Clients1/5
        [HttpPut("{ClientId}")]
        public async Task<IActionResult> PutClient(int ClientId, Client client)
        {
            if (ClientId != client.ClientId)
            {
                return BadRequest(new { message = "Client ID in the URL does not match the ID in the request body." });
            }

            _context.Entry(client).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClientExists(ClientId))
                {
                    return NotFound(new { message = $"Client with ID {ClientId} not found for update." });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Clients1/fetchClient
        [HttpPost("fetchClient")]
        public async Task<ActionResult<object>> FetchClient([FromBody] ClientLoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.ClientId) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Client ID and password are required." });
            }

            var client = await _context.Clients
                .Where(c => c.ClientId.ToString() == request.ClientId && c.ClientPassword == request.Password)
                .Select(c => new
                {
                    c.ClientId,
                    c.ClientFirstName,
                    c.ClientLastName,
                    c.Phonenumber,
                    c.Email,
                    Balance = _context.Accounts
                        .Where(a => a.ClientId == c.ClientId)
                        .Select(a => a.TotalAmount)
                        .FirstOrDefault() ?? 0
                })
                .FirstOrDefaultAsync();

            if (client == null)
            {
                return NotFound(new { message = "Invalid client ID or password." });
            }

            return Ok(client);
        }

        // POST: api/Clients1
        [HttpPost]
        public async Task<ActionResult<Client>> PostClient(Client client)
        {
            if (client == null)
            {
                return BadRequest(new { message = "Client data cannot be null." });
            }

            try
            {
                _context.Clients.Add(client);
                await _context.SaveChangesAsync();

                // Return 201 Created with the URI of the new resource
                return CreatedAtAction(nameof(GetClient), new { ClientId = client.ClientId }, client);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while creating the client.", details = ex.Message });
            }
        }

        // DELETE: api/Clients1/5
        [HttpDelete("{ClientId}")]
        public async Task<IActionResult> DeleteClient(int ClientId)
        {
            var client = await _context.Clients.FindAsync(ClientId);
            if (client == null)
            {
                return NotFound(new { message = $"Client with ID {ClientId} not found for deletion." });
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ClientExists(int ClientId)
        {
            return _context.Clients.Any(e => e.ClientId == ClientId);
        }
    }
}


