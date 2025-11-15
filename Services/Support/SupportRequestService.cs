using ReadVideo.Server.Models;

namespace ReadVideo.Server.Services.Support
{
    public class SupportRequestService : ISupportRequestService
    {
        private readonly ILogger<SupportRequestService> _logger;
        private readonly IEmailService _emailService;

        public SupportRequestService(
            ILogger<SupportRequestService> logger,
            IEmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<SupportRequestResponseDto> ProcessSupportRequestAsync(SupportRequestDto request)
        {
            // Generate unique ticket ID
            var ticketId = $"SUPPORT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

            // Send notification email to support team with Datacol XML attached
            await _emailService.SendSupportNotificationAsync(
                ticketId,
                request.Email,
                request.Question,
                request.DatacolXml,
                request.CurrentUrl);

            // Send auto-reply confirmation email to user
            await _emailService.SendConfirmationEmailAsync(request.Email, ticketId);

            _logger.LogInformation("Support request {TicketId} processed successfully for {Email}", ticketId, request.Email);

            return new SupportRequestResponseDto
            {
                Success = true,
                Message = "Your support request has been received successfully.",
                TicketId = ticketId,
                ReceivedAt = DateTime.UtcNow
            };
        }
    }
}
