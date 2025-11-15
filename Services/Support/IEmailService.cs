namespace ReadVideo.Server.Services.Support
{
    public interface IEmailService
    {
        Task SendSupportNotificationAsync(string ticketId, string email, string question, string datacolXml, string currentUrl);
        Task SendConfirmationEmailAsync(string email, string ticketId);
    }
}
