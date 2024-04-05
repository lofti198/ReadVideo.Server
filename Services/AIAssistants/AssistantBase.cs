namespace ReadVideo.Server.Services.AIAssistants
{
    public abstract class AssistantBase : IAssistant
    {
        public abstract Task<string> GetResponseAsync(string userInput, string assistantId, string threadId);
    }

}
