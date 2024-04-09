namespace ReadVideo.Server.Services.AIAssistants
{
    public interface IAssistant
    {
        Task<string> GetResponseAsync(string userInput, string additionaInstruction, string assistantId, string threadId);
    }
}