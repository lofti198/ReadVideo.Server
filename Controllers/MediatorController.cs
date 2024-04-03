using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ReadVideo.Server.Data;
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
        // private readonly IOpenAIService _openAIService; // Service to interact with OpenAI API

        public MediatorController()//IJivoService jivoService, IOpenAIService openAIService)
        {
            //_jivoService = jivoService;
            //_openAIService = openAIService;
        }
       
        [HttpPost("StartSpeaking")]
        public async Task<IActionResult> StartSpeaking([FromBody] ClientMessage chatRequest)
        {
            // https://www.jivo.ru/docs/bot/
            // Create the response object and populate it
            var botResponse = new BotResponse
            {
                Id = Guid.NewGuid().ToString(), // Generating a new unique GUID
                ClientId = chatRequest.ClientId, // Using the client ID from the request
                ChatId = chatRequest.ChatId, // Using the chat ID from the request
                Event = "BOT_MESSAGE"
            };

            // Setting the message details
            botResponse.Message.Type = "TEXT";
            botResponse.Message.Text = "Я Mock! Чем могу вам помочь?";
            botResponse.Message.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // Current timestamp

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
