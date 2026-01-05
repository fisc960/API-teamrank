
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Security.Cryptography;
    using System.Text;
    using GemachApp.Data;


namespace GemachApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.PasswordHash == request.Password);

            if (admin == null || !VerifyPassword(request.Password, admin.PasswordHash))
            {
                return Unauthorized("Invalid credentials");
            }

            return Ok(new { message = "Login successful" });
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var hashString = BitConverter.ToString(hash).Replace("-", "").ToLower();
                return hashString == storedHash;
            }
        }
    }

    public class AdminLoginRequest
    {
        public string?Password{ get; set; }
    }

}

