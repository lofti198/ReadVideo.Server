using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ReadVideo.Server.Data;
using System.Net;

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
            // Create the response object and populate it
            var botResponse = new BotResponse
            {
                Id = Guid.NewGuid().ToString(), // Generating a new unique GUID
                ClientId = chatRequest.ClientId, // Using the client ID from the request
                ChatId = chatRequest.ChatId, // Using the chat ID from the request
                Event = "BOT_MESSAGE"
            };

            // Setting the message details
            botResponse.Message.Text = "Здравствуйте! Чем могу вам помочь?";
            botResponse.Message.Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // Current timestamp

            // Serialize the BotResponse object to a JSON string
            var jsonResponse = JsonConvert.SerializeObject(botResponse);

            // Return the serialized JSON string in the response
            return Ok(jsonResponse);
        }
    }
}
