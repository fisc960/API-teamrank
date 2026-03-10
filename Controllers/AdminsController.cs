using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using GemachApp.Data;

namespace GemachApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher<Admin> _hasher = new();

        public AdminsController(AppDbContext context)
        {
            _context = context;
        }

        // ADMIN LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login(AdminLoginDto dto)
        {
            try
            {
                var admin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.Name == dto.Name);

                if (admin == null)
                    return Unauthorized("Invalid credentials");

                var result = _hasher.VerifyHashedPassword(
                    admin,
                    admin.PasswordHash,
                    dto.Password
                );

                if (result == PasswordVerificationResult.Failed)
                    return Unauthorized("Invalid credentials");

                return Ok(new
                {
                    admin.Id,
                    admin.Name
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, "Login failed");
            }
        }

        // CHANGE PASSWORD
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var admin = await _context.Admins.FindAsync(dto.AdminId);
            if (admin == null)
                return NotFound();

            var verify = _hasher.VerifyHashedPassword(
                admin,
                admin.PasswordHash,
                dto.OldPassword
            );

            if (verify == PasswordVerificationResult.Failed)
                return Unauthorized("Old password incorrect");

            admin.PasswordHash = _hasher.HashPassword(admin, dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok("Password updated");
        }
    }

    // DTOs
    public class AdminLoginDto
    {
        public string Name { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class ChangePasswordDto
    {
        public int AdminId { get; set; }
        public string OldPassword { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }
}

