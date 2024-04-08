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
    }


    public interface IJivoSiteService
    {
        Task<string> SendMessageAsync(string clientId, string chatId, string messageText);
    }

}
