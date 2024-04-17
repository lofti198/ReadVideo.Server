using Microsoft.AspNetCore.Mvc;
using ReadVideo.Server.Data;
using ReadVideo.Server.Services.BotStateManagement;
using ReadVideo.Server.Utils;
using System.Diagnostics;

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
        //[HttpPost("StartSpeaking")]
        //public async Task<IActionResult> StartSpeaking([FromBody] ClientMessage clientMessage)
        //{
        //    return await JivoMediator(clientMessage, "startspeaking");
        //}

        [HttpPost("Datacol")]
        public async Task<IActionResult> Datacol([FromBody] ClientMessage clientMessage)
        {
            string serviceKey = "";
            if(clientMessage.Sender.Url.ToLower().Contains("zZzPoH2Dbm".ToLower()) ||
                clientMessage.Sender.Url.ToLower().Contains("web-data-extractor.net"))
            {
                serviceKey = "datacol";
            }
            else if (clientMessage.Sender.Url.ToLower().Contains("startspeaking.space"))
            {
                serviceKey = "startspeaking";
            }
            if (String.IsNullOrEmpty(serviceKey))
            {
                throw new Exception("Empty service key");
            }
            return await JivoMediator(clientMessage, serviceKey);
        }      

        public async Task<IActionResult> JivoMediator(ClientMessage clientMessage, string serviceKey)
        {
           
            string log = $"Call JivoMediator Url = {clientMessage.Sender.Url}, SiteId = {clientMessage.SiteId}, ChatId = {clientMessage.ChatId},ClientId = {clientMessage.ClientId}, ButtonId = {clientMessage.Message.ButtonId},Message = {clientMessage.Message.Text}";
            Console.WriteLine(log);
            Debug.WriteLine(log);

            
            // Immediately return OK result
            Task.Run(() => ProcessMessageInBackground(clientMessage, serviceKey));

            return Ok();
        }

        private async Task ProcessMessageInBackground(ClientMessage clientMessage, string serviceKey)
        {
            try
            {                
                ClientToBotMessage clientToBotMessage = new ClientToBotMessage(clientMessage.Message.Text, clientMessage.Message.ButtonId, serviceKey, clientMessage.ClientId, clientMessage.ChatId,
                    clientMessage.Sender.Name);

                BotStateManager botStateManager = _botStateManagerFactory.GetOrCreate(serviceKey);
                
                await botStateManager.FindAppropriateAndExecute(clientToBotMessage);

                Debug.WriteLine("Processed in BG");
                Console.WriteLine("Processed in BG");
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
