namespace ReadVideo.Server.Services.AIAssistants
{
    public abstract class AssistantServiceBase : IAssistantServiceBase
    {
        public abstract Task<string> GetResponseAsync(string userInput, string assistantId, string threadId);
    }

}
