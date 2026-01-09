
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GemachApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Azure.Core;

namespace GemachApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<Admin> _passwordHasher = new();

        public AdminsController(AppDbContext context)
        {
            _context = context;
        }


        // ADMIN LOGIN (HASHED)

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginDto dto)
        {
            var admin = await _context.Admins
                .FirstOrDefaultAsync(a => a.Name == dto.Name);

            if (admin == null)
                return Unauthorized("Invalid credentials");

            var hasher = new PasswordHasher<Admin>();

            var result = hasher.VerifyHashedPassword(
                admin,
                admin.PasswordHash,
                dto.Password
            );

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized("Invalid credentials");


            return Ok(admin);
        }

        // AGENT LOGIN (PLAIN)

        [HttpPost("agent-login")]
        public IActionResult AgentLogin([FromBody] NewAgent dto)
        {
            var agent = _context.Agents
                .FirstOrDefault(a =>
                    a.AgentName == dto.Name &&
                    a.AgentPassword == dto.Password
                );

            if (agent == null)
                return Unauthorized("Invalid credentials");

            return Ok(new
            {
                agentId = agent.Id,
                agentName = agent.AgentName
            });
        }

        // CHANGE ADMIN PASSWORD

        [HttpPost("change-password")]
        public IActionResult ChangePassword(ChangePasswordDto dto)
        {
            var admin = _context.Admins.FirstOrDefault(a => a.Id == dto.AdminId);
            if (admin == null)
                return NotFound("Admin not found");

            var hasher = new PasswordHasher<Admin>();

            var verify = hasher.VerifyHashedPassword(
                admin,
                admin.PasswordHash,
                dto.OldPassword
            );

            if (verify == PasswordVerificationResult.Failed)
                return Unauthorized("Old password incorrect");

            admin.PasswordHash = hasher.HashPassword(admin, dto.NewPassword);
            _context.SaveChanges();

            return Ok("Password changed");
        }

        // RESET ADMIN PASSWORD (ADMIN ONLY)

        [HttpPost("reset-password")]
        public IActionResult ResetPassword(ResetPasswordDto dto)
        {
            var admin = _context.Admins.FirstOrDefault(a => a.Id == dto.AdminId);
            if (admin == null)
                return NotFound("Admin not found");

            var hasher = new PasswordHasher<Admin>();
            admin.PasswordHash = hasher.HashPassword(admin, dto.NewPassword);

            _context.SaveChanges();

            return Ok("Password reset");
        }
    }

    // DTOs
    public class AdminLoginDto
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }

    public class ChangePasswordDto
    {
        public int AdminId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class ResetPasswordDto
    {
        public int AdminId { get; set; }
        public string NewPassword { get; set; }
    }
}

