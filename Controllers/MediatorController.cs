using Amazon.Runtime;
using HigLabo.OpenAI;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ReadVideo.Server.Data;
using ReadVideo.Server.Services;
using ReadVideo.Server.Services.AIAssistants;
using ReadVideo.Server.Services.Embeddings;
using ReadVideo.Server.Services.Embeddings.Storage;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

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
        private readonly IChatDataStorageService _chatDataStorage;
        private readonly IEmbeddingManager _embeddingManager;
        private readonly IHelloMessageDetector _helloMessageDetector;

        public MediatorController(IAssistant assistant, IHttpClientFactory httpClientFactory,
            IJivoSiteService jivoSiteService, IChatDataStorageService chatDataStorage,
            IEmbeddingManager embeddingManager,
            IHelloMessageDetector helloMessageDetector)//IJivoService jivoService, IOpenAIService openAIService)
        {
            _assistant = assistant;
            _httpClientFactory = httpClientFactory;
            _jivoSiteService = jivoSiteService;
            _chatDataStorage = chatDataStorage;
            _embeddingManager = embeddingManager;
            _helloMessageDetector = helloMessageDetector;
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

        //public async Task<IActionResult> JivoMediator(ClientMessage clientMessage, string assistantId)
        //{
        //    Console.WriteLine($"Call JivoMediator assistantId = {assistantId}, ChatId = {clientMessage.ChatId}, Message = {clientMessage.Message.Text}");
        //    Debug.WriteLine($"Call JivoMediator assistantId = {assistantId}, ChatId = {clientMessage.ChatId}, Message = {clientMessage.Message.Text}");

        //    string openAIResponseText = await _assistant.GetResponseAsync(clientMessage.Message.Text, assistantId, clientMessage.ChatId);

        //    var responseContent = await _jivoSiteService.SendMessageAsync(clientMessage.ClientId, clientMessage.ChatId, openAIResponseText);

        //    var combinedResponse = new
        //    {
        //        JivoSiteResponse = responseContent,
        //        OpenAIResponse = openAIResponseText
        //    };

        //    return Ok(combinedResponse);

        //}

        public async Task<IActionResult> JivoMediator(ClientMessage clientMessage, string assistantId)
        {
           
            string log = $"Call JivoMediator assistantId = {assistantId}, ChatId = {clientMessage.ChatId},ClientId = {clientMessage.ClientId}, ButtonId = {clientMessage.Message.ButtonId},Message = {clientMessage.Message.Text}";
            Console.WriteLine(log);
            Debug.WriteLine(log);

            // Immediately return OK result
            Task.Run(() => ProcessMessageInBackground(clientMessage));

            return Ok();
        }


        private async Task ProcessMessageInBackground(ClientMessage clientMessage)
        {
            try
            {
                // _chatDataStorage
                // User choose to invite operator
                if (clientMessage.Message.ButtonId == 1)
                {
                    Console.WriteLine($"Invite agent");
                    await _jivoSiteService.InviteAgentAsync(clientMessage.ClientId, clientMessage.ChatId);                    
                }
                else if (clientMessage.Message.ButtonId == 2)
                {
                    Console.WriteLine($"Ask assistant to extract answer");

                    ChatData chatData = _chatDataStorage.GetLastData(clientMessage.ClientId, clientMessage.ChatId);
                    string assistantResponse = await _assistant.GetResponseAsync(chatData.UserRequest,
                        chatData.FaqItems.BuildAssistantInstruction(), 
                        Consts.OpenAIAssistantID_DC, clientMessage.ChatId);

                    await _jivoSiteService.SendMessageAsync(clientMessage.ClientId, clientMessage.ChatId, assistantResponse);
                    // Call assistant here, passing last RAG
                    await _jivoSiteService.SendMessageWithButtonsAsync(clientMessage.ClientId, clientMessage.ChatId,
                        "Invite Assistant", "text",
                        new List<Button>() {
                            new Button() { Text = "yes", Id = 1 },
                        });
                }
                else
                {
                    // Check if just hello
                    bool justHello = false;// await _helloMessageDetector.IsHelloMessage(clientMessage.Message.Text);
                    // here
                    if(justHello)
                    {
                        await _jivoSiteService.SendMessageAsync(clientMessage.ClientId, clientMessage.ChatId, 
                            "Я AI помощник поддержки Datacol. Пожалуйста, задайте свой вопрос");
                    }
                    else
                    {
                        List<FAQItem> faqLinks = await _embeddingManager.GetResponseAsync(clientMessage.Message.Text);

                        _chatDataStorage.SaveData(clientMessage.ClientId, clientMessage.ChatId, new ChatData()
                        {
                            FaqItems = faqLinks,
                            UserRequest = clientMessage.Message.Text
                        });
                        //await _assistant.GetResponseAsync(clientMessage.Message.Text,"", Consts.OpenAIAssistantID_DC, clientMessage.ChatId);

                        await _jivoSiteService.SendMessageAsync(clientMessage.ClientId, clientMessage.ChatId, faqLinks.BuildFAQReference());

                        await _jivoSiteService.SendMessageWithButtonsAsync(clientMessage.ClientId, clientMessage.ChatId,
                            "Отправить вопрос в поддержку или Сделать саммари с помощью AI", "text",
                            new List<Button>() {
                            new Button() { Text = "В поддержку", Id = 1 },
                            new Button() { Text = "Саммари", Id = 2 }
                            });
                    }
                    
                }
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
