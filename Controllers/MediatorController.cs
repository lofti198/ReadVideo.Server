using Amazon.Runtime;
using HigLabo.OpenAI;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ReadVideo.Server.Data;
using ReadVideo.Server.Services;
using ReadVideo.Server.Services.AIAssistants;
using ReadVideo.Server.Services.Embeddings;
using ReadVideo.Server.Services.Embeddings.Storage;
using ReadVideo.Server.Utils;
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
        // private readonly IJivoSiteService _jivoSiteService;
        DITypeFactoryBase<string, IJivoSiteService> _jivoSiteServiceFactory;
        private readonly IChatDataStorageService _chatDataStorage;
        private readonly IEmbeddingManager _embeddingManager;
        private readonly IMessageEvaluator _messageEvaluator;

        public MediatorController(IAssistant assistant, IHttpClientFactory httpClientFactory,
            IChatDataStorageService chatDataStorage,
            IEmbeddingManager embeddingManager,
            IMessageEvaluator messageEvaluator, DITypeFactoryBase<string, IJivoSiteService> jivoSiteServiceFactory)//IJivoService jivoService, IOpenAIService openAIService)
        {
            _assistant = assistant;
            _httpClientFactory = httpClientFactory;
  
            _chatDataStorage = chatDataStorage;
            _embeddingManager = embeddingManager;
            _messageEvaluator = messageEvaluator;
            _jivoSiteServiceFactory = jivoSiteServiceFactory;
            //_jivoService = jivoService;
            //_openAIService = openAIService;
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
                var jivoSiteService = _jivoSiteServiceFactory.GetOrCreate(token);
                ChatData chatData = _chatDataStorage.GetChatData(clientMessage.ClientId, clientMessage.ChatId);
                ClientToBotMessage clientToBotMessage = new ClientToBotMessage(clientMessage.Message.Text, clientMessage.Message.ButtonId);

                // ClientToBotMessage prevQuestion = chatData.GetLastMessage();
                ClientToBotMessage LastMessage = chatData.GetLastMessage();
                if (LastMessage != null)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(LastMessage));
                }
                ClientToBotMessage LastTextMessage = chatData.GetLastMessage(false);
                if (LastTextMessage != null)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(LastTextMessage));
                }
                // _chatDataStorage
                // User choose to invite operator
                if (clientMessage.Message.ButtonId == 1)
                {
                    Console.WriteLine($"Invite agent");
                    await jivoSiteService.InviteAgentAsync(clientMessage.ClientId, clientMessage.ChatId);
                }
                else if (clientMessage.Message.ButtonId == 10)
                {
                    await jivoSiteService.SendMessageAsync(clientMessage.ClientId, clientMessage.ChatId, "Какой сайт вы хотите парсить?");
                }
                else if (clientMessage.Message.ButtonId == 50)
                {
                    await jivoSiteService.SendMessageAsync(clientMessage.ClientId, clientMessage.ChatId,
                        "Перейдите, пожалуйста [по ссылке](https://web-data-extractor.net/buy/). Есть ли у вас еще вопросы?");

                }
                else if (LastTextMessage != null && clientMessage.Message.ButtonId == 20)
                {
                    await Task.Delay(1000);
                    await jivoSiteService.SendMessageAsync(clientMessage.ClientId, clientMessage.ChatId, "Пару секунд, AI готовит ответ...");

                    Console.WriteLine($"Ask assistant to extract answer");

                    //string assistantResponse = await _assistant.GetResponseAsync(LastTextMessage.Text,
                    //    LastTextMessage.FaqItems.BuildAssistantInstruction(),
                    //    Consts.OpenAIAssistantID_DC, clientMessage.ChatId);

                    await jivoSiteService.SendMessageAsync(clientMessage.ClientId, clientMessage.ChatId, LastTextMessage.FaqItems.BuildFAQReference());

                    await jivoSiteService.SendMessageWithButtonsAsync(clientMessage.ClientId, clientMessage.ChatId,
                     "Есть ли у вас еще вопросы? Также, возможно, вы хотите: ", "text",
                     new List<Button>() {
                            new Button() { Text = "Описать задачу по парсингу", Id = 10 },
                            // new Button() { Text = "Купить программу", Id = 50 }
                     });

                    // await jivoSiteService.SendMessageAsync(clientMessage.ClientId, clientMessage.ChatId, "Есть ли у вас еще вопросы?");

                    // Call assistant here, passing last RAG
                    //await jivoSiteService.SendMessageWithButtonsAsync(clientMessage.ClientId, clientMessage.ChatId,
                    //    "Invite Assistant", "text",
                    //    new List<Button>() {
                    //        new Button() { Text = "yes", Id = 1 },
                    //    });

                }
                else
                {
                    // choose site
                    if(LastMessage !=null && LastMessage.ButtonId==10)
                    {
                        clientToBotMessage.SpecialId = "10.1:site_input";
                        await jivoSiteService.SendMessageAsync(clientMessage.ClientId, clientMessage.ChatId,
                                "Прекрасно, скажи, какие данные нужно собрать?");
                    }
                    else if(LastMessage != null && LastMessage.SpecialId == "10.1:site_input")
                    {
                        //await jivoSiteService.SendMessageAsync(clientMessage.ClientId, clientMessage.ChatId,
                        //        "Отлично! Записали вашу задачу. Есть ли у вас еще вопросы?");
                        await jivoSiteService.SendMessageWithButtonsAsync(clientMessage.ClientId, clientMessage.ChatId,
                     "Отлично! Записали вашу задачу. Есть ли у вас еще вопросы? Также, возможно, вы сразу хотите: ", "text",
                     new List<Button>() {
                            new Button() { Text = "Описать еще одну задачу по парсингу", Id = 10 },
                            new Button() { Text = "Купить программу", Id = 50 }
                     });
                    }
                    else
                    {
                        // Check if just hello
                        var messageFeatures = await _messageEvaluator.EvaluateMessageFeatures(clientMessage.Message.Text);
                        // here
                        if (messageFeatures.Critical)
                        {
                            await jivoSiteService.SendMessageAsync(clientMessage.ClientId, clientMessage.ChatId,
                                "Данный вопрос относится к срочным, поэтому передаю его сразу в поддержку! Наша команда свяжется с вами в течение 24 часов. Есть ли у вас еще вопросы?");

                     //       await jivoSiteService.SendMessageWithButtonsAsync(clientMessage.ClientId, clientMessage.ChatId,
                     //"Данный вопрос относится к критичным, поэтому передаю его сразу в поддержку! Наша команда свяжется с вами в течение 24 часов. Есть ли у вас еще вопросы? Также, возможно, вы сразу хотите: ", "text",
                     //new List<Button>() {
                     //       new Button() { Text = "Описать задачу по парсингу", Id = 10 },
                     //       new Button() { Text = "Купить программу", Id = 50 }
                     //       // TODO
                        }
                        else if (!messageFeatures.Complex)
                        {
                            //await jivoSiteService.SendMessageAsync(clientMessage.ClientId, clientMessage.ChatId,
                            //    "Я AI помощник Datacol. Пожалуйста, задайте свой вопрос. Также, возможно, вы сразу хотите: ");

                            await jivoSiteService.SendMessageWithButtonsAsync(clientMessage.ClientId, clientMessage.ChatId,
                     "Я AI помощник Datacol. Пожалуйста, задайте свой вопрос. Также, возможно, вы сразу хотите: ", "text",
                     new List<Button>() {
                            new Button() { Text = "Описать задачу по парсингу", Id = 10 },
                            new Button() { Text = "Купить программу", Id = 50 }
                     });
                        }
                        else
                        {
                            await Task.Delay(1000);
                            await jivoSiteService.SendMessageAsync(clientMessage.ClientId, clientMessage.ChatId, "Пару секунд, AI готовит ответ...");

                            List<FAQItem> faqLinks = await _embeddingManager.GetResponseAsync(clientMessage.Message.Text);
                            clientToBotMessage.FaqItems = faqLinks;
                            //await _assistant.GetResponseAsync(clientMessage.Message.Text,"", Consts.OpenAIAssistantID_DC, clientMessage.ChatId);

                            string assistantResponse = await _assistant.GetResponseAsync(clientMessage.Message.Text,
                                faqLinks.BuildAssistantInstruction(),
                                Consts.OpenAIAssistantID_DC, clientMessage.ChatId);

                            await jivoSiteService.SendMessageAsync(clientMessage.ClientId, clientMessage.ChatId, assistantResponse);
                            await Task.Delay(1000);
                            await jivoSiteService.SendMessageWithButtonsAsync(clientMessage.ClientId, clientMessage.ChatId,
                                "Удалось ли найти ответ? Если нет, то могу переслать вопрос на email поддержки или показать похожие статьи из базы знаний. Также, вы можете сформулировать вопрос по-другому либо сразу описать свою задачу по парсингу.", "text",
                                new List<Button>() {
                            new Button() { Text = "Переслать поддержке", Id = 1 },
                            new Button() { Text = "Показать похожие", Id = 20 },
                            new Button() { Text = "Описать задачу по парсингу", Id = 10 }
                                });
                        }

                    }

                }

               
                _chatDataStorage.SaveData(clientMessage.ClientId, clientMessage.ChatId, clientToBotMessage);
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
