namespace ReadVideo.Server.Models
{
    public class SupportRequestResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? TicketId { get; set; }
        public DateTime ReceivedAt { get; set; }
    }
}
