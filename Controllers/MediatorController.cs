using HigLabo.OpenAI;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ReadVideo.Server.Data;
using ReadVideo.Server.Services.AIAssistants;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ReadVideo.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediatorController : ControllerBase
    {
        // private readonly IJivoService _jivoService; // Service to interact with Jivo API
        private readonly IAssistantServiceBase _assistant; // Service to interact with OpenAI API

        public MediatorController(IAssistantServiceBase assistant)//IJivoService jivoService, IOpenAIService openAIService)
        {
            _assistant = assistant;
            //_jivoService = jivoService;
            //_openAIService = openAIService;
        }

        [HttpPost("StartSpeaking")]
        public async Task<IActionResult> StartSpeaking([FromBody] ClientMessage clientMessage)
        {
            return await JivoMediator(clientMessage, Consts.OpenAIAssistantID_DC);
        }

        [HttpPost("Datacol")]
        public async Task<IActionResult> Datacol([FromBody] ClientMessage clientMessage)
        {
            return await JivoMediator(clientMessage, Consts.OpenAIAssistantID_DC);
        }

        public async Task<IActionResult> JivoMediator(ClientMessage clientMessage, string assistantId)
        {
            // https://www.jivo.ru/docs/bot/
            // Extract necessary data from the CLIENT_MESSAGE


            Console.WriteLine($"Call JivoMediator assistantId = {assistantId}, ChatId = {clientMessage.ChatId}, Message = {clientMessage.Message.Text}");

            string openAIResponseText = await _assistant.GetResponseAsync(clientMessage.Message.Text, assistantId, clientMessage.ChatId);

            // Form the BOT_MESSAGE based on OpenAI's response
            var botResponse = new BotResponse
            {
                Id = Guid.NewGuid().ToString(),
                ClientId = clientMessage.ClientId,
                ChatId = clientMessage.ChatId,
                Message = new BotMessage
                {
                    Text = openAIResponseText,
                    Type = "TEXT",
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                },
                Event = "BOT_MESSAGE"
            };

            // Serialize the BotResponse object to a JSON string using System.Text.Json
            var options = new JsonSerializerOptions { WriteIndented = true };
            var jsonResponse = System.Text.Json.JsonSerializer.Serialize(botResponse, options);
            // Serialize the BotResponse object to a JSON string
            // var jsonResponse = JsonConvert.SerializeObject(botResponse);

            // Initialize HttpClient
            using (var httpClient = new HttpClient())
            {
                // Set the endpoint URL
                var url = "https://bot.jivosite.com/webhooks/t1iWHhKC6aSYEgz/startspeaking";

                // Create the HttpContent for the request
                var content = new StringContent(jsonResponse, Encoding.UTF8, "application/json");

                // Send the POST request
                var response = await httpClient.PostAsync(url, content);

                // Check the response status
                if (response.IsSuccessStatusCode)
                {
                    // Handle success
                    var responseContent = await response.Content.ReadAsStringAsync();
                    // Do something with the response, if needed
                    return Ok(responseContent);
                }
                else
                {
                    // Handle failure
                    return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
                }
            }

        }
    }
}
