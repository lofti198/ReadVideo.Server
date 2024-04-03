using HigLabo.OpenAI;
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
        public async Task<IActionResult> StartSpeaking([FromBody] ClientMessage clientMessage)
        {
            // https://www.jivo.ru/docs/bot/
            // Extract necessary data from the CLIENT_MESSAGE
            string userInput = clientMessage.Message.Text;

            // Initialize OpenAI Client
            var cl = new OpenAIClient(Environment.GetEnvironmentVariable("GPT_API_KEY"));
            string threadId = ""; // Consider storing threadId in a session or a persistent storage

            // Initialize thread on first interaction
            if (string.IsNullOrEmpty(threadId))
            {
                var threadCreationResponse = await cl.ThreadCreateAsync();
                threadId = threadCreationResponse.Id;
            }

            // Send user input as a message to the assistant
            var messageCreateParams = new MessageCreateParameter
            {
                Thread_Id = threadId,
                Role = "user",
                Content = userInput
            };
            await cl.MessageCreateAsync(messageCreateParams);

            // Prepare and get the response
            var runCreateParams = new RunCreateParameter
            {
                Assistant_Id = Consts.ASST_ID,
                Thread_Id = threadId,
                Stream = true
            };
            string openAIResponseText = "";

            // Receive the response stream
            await foreach (var text in cl.RunCreateStreamAsync(runCreateParams, new AssistantMessageStreamResult(), CancellationToken.None))
            {
                openAIResponseText += text;
            }

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
