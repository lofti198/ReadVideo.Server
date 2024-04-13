using Amazon.Runtime;
using HigLabo.OpenAI;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ReadVideo.Server.Data;
using ReadVideo.Server.Services;
using ReadVideo.Server.Services.AIAssistants;
using ReadVideo.Server.Services.BotStateManagement;
using ReadVideo.Server.Services.EmailSending;
using ReadVideo.Server.Services.Embeddings;
using ReadVideo.Server.Services.Embeddings.Storage;
using ReadVideo.Server.Utils;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using IEmailSender = ReadVideo.Server.Services.EmailSending.IEmailSender;

namespace ReadVideo.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediatorController : ControllerBase
    {
        private readonly DITypeFactoryBase<string, BotStateManager> _botStateManagerFactory;
        

        public MediatorController(
            DITypeFactoryBase<string, BotStateManager> botStateManagerFactory)
        {
            
            _botStateManagerFactory = botStateManagerFactory;
        }

        // [HttpGet("SomeAction/{key}")]
        [HttpPost("StartSpeaking")]
        public async Task<IActionResult> StartSpeaking([FromBody] ClientMessage clientMessage)
        {
            return await JivoMediator(clientMessage, "startspeaking");
        }

        [HttpPost("Datacol")]
        public async Task<IActionResult> Datacol([FromBody] ClientMessage clientMessage)
        {
            return await JivoMediator(clientMessage,  "datacol");
        }

      

        public async Task<IActionResult> JivoMediator(ClientMessage clientMessage, string token)
        {
           
            string log = $"Call JivoMediator Url = {clientMessage.Sender.Url}, SiteId = {clientMessage.SiteId}, ChatId = {clientMessage.ChatId},ClientId = {clientMessage.ClientId}, ButtonId = {clientMessage.Message.ButtonId},Message = {clientMessage.Message.Text}";
            Console.WriteLine(log);
            Debug.WriteLine(log);

            
            // Immediately return OK result
            Task.Run(() => ProcessMessageInBackground(clientMessage, token));

            return Ok();
        }


        private async Task ProcessMessageInBackground(ClientMessage clientMessage, string token)
        {
            try
            {
                
                ClientToBotMessage clientToBotMessage = new ClientToBotMessage(clientMessage.Message.Text, clientMessage.Message.ButtonId,token, clientMessage.ClientId, clientMessage.ChatId);

                BotStateManager botStateManager = _botStateManagerFactory.GetOrCreate(token);
                
                await botStateManager.FindAppropriateAndExecute(clientToBotMessage);

             


                //_chatDataStorage.SaveData(clientMessage.ClientId, clientMessage.ChatId, clientToBotMessage);
                Debug.WriteLine("Processed in BG");
                Console.WriteLine("Processed in BG");
                // Log success or perform any follow-up actions
            }
            catch (Exception ex)
            {
                // Log the error
                Debug.WriteLine($"Error processing message in background: {ex.Message}");
                Console.WriteLine($"Error processing message in background: {ex.Message}");
               
            }
        }
    }
}
