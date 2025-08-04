using MailKit.Net.Smtp;
using MimeKit;

namespace kebabBackend.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;
        private readonly string _templateBasePath;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
            _templateBasePath = Path.Combine(AppContext.BaseDirectory, "Templates");
        }

        public async Task SendHtmlEmail(string toEmail, string subject, string templateFileName, Dictionary<string, string> placeholders)
        {
            var templatePath = Path.Combine(_templateBasePath, templateFileName);
            _logger.LogInformation("Wysyłka e-maila: szablon = {Path}", templatePath);

            if (!File.Exists(templatePath))
            {
                _logger.LogError("Nie znaleziono szablonu e-maila: {Path}", templatePath);
                throw new FileNotFoundException("Szablon e-maila nie istnieje", templatePath);
            }

            string template = await File.ReadAllTextAsync(templatePath);

            foreach (var kv in placeholders)
            {
                template = template.Replace($"{{{{{kv.Key}}}}}", kv.Value);
            }

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_config["Email:From"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = template };

            using var client = new SmtpClient();
            client.ServerCertificateValidationCallback = (s, c, h, e) => true; 
            await client.ConnectAsync(_config["Email:SmtpServer"], int.Parse(_config["Email:Port"]), true);
            await client.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("E-mail wysłany do: {Recipient}", toEmail);
        }
    }
}
