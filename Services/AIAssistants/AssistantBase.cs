namespace ReadVideo.Server.Services.AIAssistants
{
    public abstract class AssistantBase : IAssistant
    {
        public abstract Task<string> GetResponseAsync(string userInput, string additionaInstruction, string assistantId, string threadId);
    }

}
