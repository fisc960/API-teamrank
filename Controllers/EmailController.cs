using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using GemachApp.Data;
using Microsoft.Extensions.Configuration;

namespace GemachApp.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EmailController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private readonly IEmailService _email;

        public EmailController(IEmailService email)
        {
            _email = email;
        }

        [HttpPost("SendEmail")]
        public IActionResult SendEmail([FromBody] EmailRequest emailRequest)
        {
            Console.WriteLine($"Received Email Request: {emailRequest?.To}, {emailRequest?.Subject}, {emailRequest?.Body}");

            try
            {

                if (emailRequest == null || string.IsNullOrEmpty(emailRequest.To) ||
    string.IsNullOrEmpty(emailRequest.Subject) || string.IsNullOrEmpty(emailRequest.Body))
                {
                    return BadRequest("Missing required email fields.");
                }


                // Retrieve configuration values
                string smtpServer = _configuration["SmtpSettings:Server"] ?? "smtp.gmail.com"; // Default fallback
                if (!int.TryParse(_configuration["SmtpSettings:Port"], out int smtpPort))
                {
                    return BadRequest("Invalid SMTP port in configuration.");
                }


                string smtpUsername = _configuration["SmtpSettings:Username"];
                string smtpPassword = _configuration["SmtpSettings:Password"];
                string fromEmail = _configuration["SmtpSettings:FromEmail"];

                var smtpClient = new SmtpClient(smtpServer)
                {
                    Port = smtpPort,
                    //Credentials = new NetworkCredential("poplo.fisch@gmail.com\r\n", "zaQ!xsw2cde3"),
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    //EnableSsl = true, // For port 587
                    EnableSsl = false, 
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                // Set credentials and enable SSL *after* initializing smtpClient
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                smtpClient.EnableSsl = true;

                Console.WriteLine($"SMTP Server: {smtpServer}, Port: {smtpPort}");
                Console.WriteLine($"Username: {smtpUsername}, From Email: {fromEmail}");


                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = emailRequest.Subject,
                    Body = emailRequest.Body,
                    IsBodyHtml = false,
                };

                mailMessage.To.Add(emailRequest.To);
                smtpClient.Send(mailMessage);

                return Ok("Email sent successfully");
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine($"SMTP Error: {smtpEx.StatusCode} - {smtpEx.Message}");
                return BadRequest($"SMTP Error: {smtpEx.StatusCode} - {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Error: {ex.Message}");
                return BadRequest("General Error: " + ex.Message);
            }

        }
    }
}
