namespace ReadVideo.Server.Services
{
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Amazon.Runtime.Internal.Util;
    using Newtonsoft.Json;
    using ReadVideo.Server.Data;

    // https://www.jivo.ru/docs/bot/
    public class JivoSiteService : IJivoSiteService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public JivoSiteService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // New method to send a message with buttons
        public async Task<string> SendMessageWithButtonsAsync(string clientId, string chatId, string title, string text, List<Button> buttons)
        {
            // https://www.jivo.ru/docs/bot/#bot-message
            var botResponse = new BotResponse
            {
                Id = Guid.NewGuid().ToString(),
                ClientId = clientId,
                ChatId = chatId,
                Message = new BotMessageWithButtons
                {
                    Title = title,
                    Text = text,
                    Type = "BUTTONS",
                    ForceReply = true,
                    Buttons = buttons,
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                },
                Event = "BOT_MESSAGE"
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new BotMessageConverter() }
            };
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(botResponse, options);

            var httpClient = _httpClientFactory.CreateClient();
            var url = "https://bot.jivosite.com/webhooks/t1iWHhKC6aSYEgz/startspeaking";
            var content = new StringContent(jsonResponse, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new HttpRequestException($"Request to JivoSite failed with status code: {response.StatusCode}");
            }
        }

        public async Task<string> SendMessageAsync(string clientId, string chatId, string messageText)
        {
            var botResponse = new BotResponse
            {
                Id = Guid.NewGuid().ToString(),
                ClientId = clientId,
                ChatId = chatId,
                Message = new BotMessage
                {
                    Text = $"TEXT: {messageText}",
                    Content = messageText,
                    Type = "MARKDOWN",
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                },
                Event = "BOT_MESSAGE"
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(botResponse, options);

            var httpClient = _httpClientFactory.CreateClient();
            var url = "https://bot.jivosite.com/webhooks/t1iWHhKC6aSYEgz/startspeaking";
            var content = new StringContent(jsonResponse, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new HttpRequestException($"Request to JivoSite failed with status code: {response.StatusCode}");
            }
        }
        public async Task<string> InviteAgentAsync(string clientId, string chatId)
        {
            var botResponse = new BotResponse
            {
                Id = Guid.NewGuid().ToString(), // Unique identifier for the request
                ClientId = clientId,            // Client ID from the request
                ChatId = chatId,                // Chat ID from the request
                Event = "INVITE_AGENT"          // Event type to invite an agent
            };

            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(botResponse, options);

            var httpClient = _httpClientFactory.CreateClient();
            var url = "https://bot.jivosite.com/webhooks/t1iWHhKC6aSYEgz/startspeaking"; // Assuming the same endpoint
            var content = new StringContent(jsonResponse, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new HttpRequestException($"Request to JivoSite failed with status code: {response.StatusCode}");
            }
        }

    }


    public interface IJivoSiteService
    {
        Task<string> SendMessageAsync(string clientId, string chatId, string messageText);

        Task<string> SendMessageWithButtonsAsync(string clientId, string chatId, string title, string text, List<Button> buttons);

        Task<string> InviteAgentAsync(string clientId, string chatId);
    }

}
