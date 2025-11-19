using System.Net;
using System.Net.Mail;
using System.Text;
using ReadVideo.Server.Services.EmailSending;

namespace ReadVideo.Server.Services.Support
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly string _supportEmail;

        public EmailService(SmtpSettings smtpSettings, ILogger<EmailService> logger, IConfiguration configuration)
        {
            _smtpSettings = smtpSettings;
            _logger = logger;
            _supportEmail = configuration["SupportEmail"] ?? "support@example.com";
        }

        public async Task SendSupportNotificationAsync(string ticketId, string email, string question, string datacolXml, string currentUrl)
        {
            var subject = $"New Support Request - {ticketId}";

            var bodyBuilder = new StringBuilder();
            bodyBuilder.AppendLine("<html><body>");
            bodyBuilder.AppendLine($"<h2>New Support Request Received</h2>");
            bodyBuilder.AppendLine($"<p><strong>Ticket ID:</strong> {ticketId}</p>");
            bodyBuilder.AppendLine($"<p><strong>User Email:</strong> {email}</p>");
            bodyBuilder.AppendLine($"<p><strong>Question:</strong></p>");
            bodyBuilder.AppendLine($"<p>{System.Web.HttpUtility.HtmlEncode(question)}</p>");

            if (!string.IsNullOrEmpty(currentUrl))
            {
                bodyBuilder.AppendLine($"<p><strong>Current URL:</strong> <a href=\"{currentUrl}\">{currentUrl}</a></p>");
            }

            if (!string.IsNullOrEmpty(datacolXml))
            {
                bodyBuilder.AppendLine($"<p><strong>Note:</strong> Datacol XML configuration is attached.</p>");
            }

            bodyBuilder.AppendLine("</body></html>");

            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("noreply@web-data-extractor.net"),
                    Subject = subject,
                    Body = bodyBuilder.ToString(),
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(_supportEmail);

                // Set Reply-To as user's email so replies go directly to the user
                mailMessage.ReplyToList.Add(new MailAddress(email));

                // Add XML attachment if provided (as .par file)
                if (!string.IsNullOrEmpty(datacolXml))
                {
                    var xmlBytes = Encoding.UTF8.GetBytes(datacolXml);
                    var stream = new MemoryStream(xmlBytes);
                    var attachment = new Attachment(stream, $"{ticketId}.par", "application/xml");
                    mailMessage.Attachments.Add(attachment);
                }

                using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                    EnableSsl = _smtpSettings.EnableSsl,
                };

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Support notification email sent successfully for ticket {TicketId}", ticketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send support notification email for ticket {TicketId}", ticketId);
                throw;
            }
        }

        public async Task SendConfirmationEmailAsync(string email, string ticketId)
        {
            var subject = $"Support Request Received - {ticketId}";

            var bodyBuilder = new StringBuilder();
            bodyBuilder.AppendLine("<html><body>");
            bodyBuilder.AppendLine($"<h2>Thank you for contacting support!</h2>");
            bodyBuilder.AppendLine($"<p>We have received your support request and our team will get back to you shortly.</p>");
            bodyBuilder.AppendLine($"<p><strong>Your Ticket ID:</strong> {ticketId}</p>");
            bodyBuilder.AppendLine($"<p>Please keep this ticket ID for your reference.</p>");
            bodyBuilder.AppendLine("<br/>");
            bodyBuilder.AppendLine("<p>Best regards,<br/>Support Team</p>");
            bodyBuilder.AppendLine("</body></html>");

            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("noreply@web-data-extractor.net"),
                    Subject = subject,
                    Body = bodyBuilder.ToString(),
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);

                using var smtpClient = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port)
                {
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password),
                    EnableSsl = _smtpSettings.EnableSsl,
                };

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Confirmation email sent successfully to {Email} for ticket {TicketId}", email, ticketId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send confirmation email to {Email} for ticket {TicketId}", email, ticketId);
                throw;
            }
        }
    }
}
