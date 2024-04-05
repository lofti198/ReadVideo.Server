
using HigLabo.OpenAI;
using System.Collections.Concurrent;
using System.Text;

namespace ReadVideo.Server.Services.AIAssistants
{
    public class OpenAIAssistant : AssistantBase
    {
        public readonly OpenAIClient _client;
        private ConcurrentDictionary<string, string>_externalThreadToAssistantDict = new ConcurrentDictionary<string, string>();
        public OpenAIAssistant(OpenAIClient client)
        {
            _client = client;
        }
        public override async Task<string> GetResponseAsync(string userInput, string assistantId, string externalThreadId)
        {
            string? internalThreadId;
            _externalThreadToAssistantDict.TryGetValue(externalThreadId, out internalThreadId);
            
            // Initialize thread on first interaction
            if (string.IsNullOrEmpty(internalThreadId))
            {
                var threadCreationResponse = await _client.ThreadCreateAsync();
                internalThreadId = threadCreationResponse.Id;
                _externalThreadToAssistantDict.TryAdd(externalThreadId, internalThreadId);
            }
        
            // Send user input as a message to the assistant
            var messageCreateParams = new MessageCreateParameter
            {
                Thread_Id = internalThreadId,
                Role = "user",
                Content = userInput
            };
            await _client.MessageCreateAsync(messageCreateParams);

            // Prepare and get the response
            var runCreateParams = new RunCreateParameter
            {
                Assistant_Id = assistantId,
                Thread_Id = internalThreadId,
                Stream = true
            };
            StringBuilder openAIResponseText = new StringBuilder();

            // Receive the response stream
            await foreach (var text in _client.RunCreateStreamAsync(runCreateParams, new AssistantMessageStreamResult(), CancellationToken.None))
            {
                openAIResponseText.Append(text);
            }

            return openAIResponseText.ToString();
        }
    }
}
