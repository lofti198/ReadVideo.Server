namespace ReadVideo.Server.Services.AIAssistants
{
    public interface IAssistant
    {
        Task<string> GetResponseAsync(string userInput, string assistantId, string threadId);
    }
}