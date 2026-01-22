using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GemachApp.Data
{
    public class EmailService : IEmailService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public EmailService(
            IHttpClientFactory httpClientFactory,
            IConfiguration config)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var apiKey = _config["RESEND_API_KEY"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                Console.WriteLine("Email skipped: RESEND_API_KEY missing");
                return;
            }

            var client = _httpClientFactory.CreateClient();

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new
            {
                from = _config["EMAIL_FROM"] ?? "onboarding@resend.dev",
                to = new[] { to },
                subject = subject,
                html = body
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(
                "https://api.resend.com/emails",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Email failed: {error}");
            }
        }
    }
}