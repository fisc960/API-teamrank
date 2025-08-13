using GemachApp.Data;
using System.Net.Mail;
using System.Threading.Tasks;

namespace GemachApp.Data
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            // Your actual email sending logic here (SMTP, SendGrid, etc.)
            using var smtp = new SmtpClient("smtp.yourserver.com");
            var mail = new MailMessage("noreply@yourdomain.com", to, subject, body);
            await smtp.SendMailAsync(mail);
        }
    }
}
