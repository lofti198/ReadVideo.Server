using Amazon.Runtime;
using HigLabo.OpenAI;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ReadVideo.Server.Data;
using ReadVideo.Server.Services;
using ReadVideo.Server.Services.AIAssistants;
using System.Diagnostics;
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
        private readonly IAssistant _assistant; // Service to interact with OpenAI API
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IJivoSiteService _jivoSiteService;

        public MediatorController(IAssistant assistant, IHttpClientFactory httpClientFactory,
            IJivoSiteService jivoSiteService)//IJivoService jivoService, IOpenAIService openAIService)
        {
            _assistant = assistant;
            _httpClientFactory = httpClientFactory;
            _jivoSiteService = jivoSiteService;
            //_jivoService = jivoService;
            //_openAIService = openAIService;
        }

        [HttpPost("StartSpeaking")]
        public async Task<IActionResult> StartSpeaking([FromBody] ClientMessage clientMessage)
        {
            return await JivoMediator(clientMessage, Consts.OpenAIAssistantID_DC);
        }

        //[HttpPost("Datacol")]
        //public async Task<IActionResult> Datacol([FromBody] ClientMessage clientMessage)
        //{
        //    return await JivoMediator(clientMessage, Consts.OpenAIAssistantID_DC);
        //}

        public async Task<IActionResult> JivoMediator(ClientMessage clientMessage, string assistantId)
        {
            Console.WriteLine($"Call JivoMediator assistantId = {assistantId}, ChatId = {clientMessage.ChatId}, Message = {clientMessage.Message.Text}");
            Debug.WriteLine($"Call JivoMediator assistantId = {assistantId}, ChatId = {clientMessage.ChatId}, Message = {clientMessage.Message.Text}");

            string openAIResponseText = await _assistant.GetResponseAsync(clientMessage.Message.Text, assistantId, clientMessage.ChatId);

            var responseContent = await _jivoSiteService.SendMessageAsync(clientMessage.ClientId, clientMessage.ChatId, openAIResponseText);

            var combinedResponse = new
            {
                JivoSiteResponse = responseContent,
                OpenAIResponse = openAIResponseText
            };

            return Ok(combinedResponse);

        }
    }
}
