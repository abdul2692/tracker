using Microsoft.AspNetCore.Identity.UI.Services;

namespace SpendingTracker.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtpHost = _config["Email:SmtpHost"];
            var smtpPortStr = _config["Email:SmtpPort"];
            var smtpUser = _config["Email:SmtpUser"];
            var smtpPass = _config["Email:SmtpPass"];
            var fromEmail = _config["Email:FromEmail"] ?? "noreply@spendtracker.com";

            if (string.IsNullOrWhiteSpace(smtpHost) || 
                string.IsNullOrWhiteSpace(smtpUser) || 
                (smtpUser != null && smtpUser.Contains("YOUR_GMAIL_USERNAME")) || 
                (smtpPass != null && smtpPass.Contains("YOUR_GMAIL_APP_PASSWORD")))
            {
                // Dev mode: log the email instead of sending
                _logger.LogWarning(
                    "DEV MODE (Gmail placeholders detected) - To: {To} | Subject: {Subject}\nLink: {Body}",
                    email, subject, htmlMessage);
                return;
            }

            try
            {
                int port = int.TryParse(smtpPortStr, out var p) ? p : 587;
                using var client = new System.Net.Mail.SmtpClient(smtpHost, port)
                {
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true
                };

                var mail = new System.Net.Mail.MailMessage
                {
                    From = new System.Net.Mail.MailAddress(fromEmail, "SpendTracker"),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mail.To.Add(email);

                await client.SendMailAsync(mail);
                _logger.LogInformation("Email sent to {To}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}. Reverting to Console fallback.", email);
                _logger.LogWarning("EMAIL FALLBACK LINK - To: {To} | Body: {Body}", email, htmlMessage);
            }
        }
    }
}
