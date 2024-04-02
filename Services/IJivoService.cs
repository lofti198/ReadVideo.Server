namespace ReadVideo.Server.Services
{
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Amazon.Runtime.Internal.Util;
    using Newtonsoft.Json;


    public interface IJivoService
    {
    }

    public class JivoService : IJivoService
    {
        private readonly HttpClient _httpClient;
        private readonly string _jivoEndpoint;

        public JivoService(HttpClient httpClient, string jivoEndpoint)
        {
            _httpClient = httpClient;
            _jivoEndpoint = jivoEndpoint; // Your JivoChat Bot API endpoint
        }

        public async Task SendBotMessageAsync(string clientId, string chatId, string message)
        {
            var botMessage = new
            {
                id = Guid.NewGuid().ToString(),
                client_id = clientId,
                chat_id = chatId,
                message = new
                {
                    type = "TEXT",
                    text = message,
                },
            event1 = "BOT_MESSAGE"
        };

        string json = JsonConvert.SerializeObject(botMessage);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await _httpClient.PostAsync(_jivoEndpoint, content);
    }


    public async Task HandleClientMessageAsync(dynamic clientMessage)
    {
        // Extract needed information from clientMessage
        // For example: var text = clientMessage.message.text;

        // Process the message, interact with OpenAI or other services as needed

        // Respond back to JivoChat with a BOT_MESSAGE
        await SendBotMessageAsync(clientMessage.client_id.ToString(), clientMessage.chat_id.ToString(), "Your response here");
    }
}

}
