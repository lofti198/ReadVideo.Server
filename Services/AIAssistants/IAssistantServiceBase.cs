namespace ReadVideo.Server.Services.AIAssistants
{
    public interface IAssistantServiceBase
    {
        Task<string> GetResponseAsync(string userInput, string assistantId, string threadId);
    }
}