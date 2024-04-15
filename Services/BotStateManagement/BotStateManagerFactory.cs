using Newtonsoft.Json.Linq;
using ReadVideo.Server.Data;
using ReadVideo.Server.Services.AIAssistants;
using ReadVideo.Server.Services.EmailSending;
using ReadVideo.Server.Services.Embeddings;
using ReadVideo.Server.Utils;

namespace ReadVideo.Server.Services.BotStateManagement
{
    public class BotStateManagerFactory
    {
        private readonly DITypeFactoryBase<string, IJivoSiteService> _jivoSiteServiceFactory;
        private readonly DITypeFactoryBase<string, IEmailSender> _emailSenderServiceFactory;
        private readonly IAssistant _assistant; // Service to interact with OpenAI API
        private readonly IEmbeddingManager _embeddingManager;
        private readonly IMessageEvaluator _messageEvaluator;
        private readonly TokenToServiceKeyConverter _tokenToServiceKeyConverter;
        private readonly IChatDataStorageService _chatDataStorageService;

        public BotStateManagerFactory(DITypeFactoryBase<string, IJivoSiteService> jivoSiteServiceFactory,
            DITypeFactoryBase<string, IEmailSender> emailSenderServiceFactory, IAssistant assistant,
            IEmbeddingManager embeddingManager, IMessageEvaluator messageEvaluator, TokenToServiceKeyConverter tokenToServiceKeyConverter, IChatDataStorageService chatDataStorageService)
        {
            _jivoSiteServiceFactory = jivoSiteServiceFactory;
            _emailSenderServiceFactory = emailSenderServiceFactory;
            _assistant = assistant;
            _embeddingManager = embeddingManager;
            _messageEvaluator = messageEvaluator;
            _tokenToServiceKeyConverter = tokenToServiceKeyConverter;
            _chatDataStorageService = chatDataStorageService;
        }

        public BotStateManager Create(string key)
        {
            BotStateCollection stateCollection = new BotStateCollection();

            switch (key)
            {
                case "startspeaking":
                    stateCollection.AddState(
                        // Button "Fill applicatoin" handler
                        new BotState(
                            async (clientToBotMessage, chatHistory) => clientToBotMessage.ButtonId == 10
                            ,
                            async (clientToBotMessage, chatHistory) => {
                                await _jivoSiteServiceFactory.GetOrCreate(key).SendMessageAsync
                                    (clientToBotMessage.ClientId, clientToBotMessage.ChatId, 
                                    "How old are you? What is your profession?");

                            }
                        ));
                    stateCollection.AddState(
                        // Scenario "Describing parsing task" 1st step
                        new BotState(
                            async (clientToBotMessage, chatHistory) =>
                            {
                                var lastMessage = chatHistory.GetLastMessage();
                                if (lastMessage == null) return false;
                                return lastMessage.ButtonId == 10;
                            }
                            ,
                            async (clientToBotMessage, chatHistory) => {
                                clientToBotMessage.SpecialId = "10.1";
                                await _jivoSiteServiceFactory.GetOrCreate(key).SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                                "Great! What is your English level?");
                            }
                        ));
                    stateCollection.AddState(
                        // Scenario "Describing parsing task" 2nd step
                        new BotState(
                            async (clientToBotMessage, chatHistory) =>
                            {
                                var lastMessage = chatHistory.GetLastMessage();
                                if (lastMessage == null) return false;
                                return lastMessage.SpecialId == "10.1";
                            }
                            ,
                            async (clientToBotMessage, chatHistory) => {
                                clientToBotMessage.SpecialId = "10.2";
                                await _jivoSiteServiceFactory.GetOrCreate(key).SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                                "Finally, what is your goal learning English");
                            }
                        ));

                    stateCollection.AddState(
                        // Scenario "Describing parsing task" 2nd step (data)
                        new BotState(
                            async (clientToBotMessage, chatHistory) =>
                            {
                                var lastMessage = chatHistory.GetLastMessage();
                                if (lastMessage == null) return false;
                                return lastMessage.SpecialId == "10.2";
                            }
                            ,
                            async (clientToBotMessage, chatHistory) => {
                                // gather complete the request data!!!
                                var copmleteRequestData = Utils.ParticularBotUtils.StartSpeakingBotUtils.GetCopmleteRequestData(clientToBotMessage, chatHistory);
                                await _emailSenderServiceFactory.GetOrCreate(key).SendEmailAsync("isolar2005@gmail.com",
                                    $"{key} chat application for StartSpeaking sent (just last step for now)", copmleteRequestData);
                                await _jivoSiteServiceFactory.GetOrCreate(key).SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                                 "Great! Thank you for your time. Sasha will get back to you in 24 hours. If you have any more questions, feel free to ask!");
                                 
                            }
                        ));
                    stateCollection.AddState(
                        
                        // Other messages
                        new BotState(
                            async (clientToBotMessage, chatHistory) => true
                            ,
                            async (clientToBotMessage, chatHistory) => {
                                var jivoSiteService = _jivoSiteServiceFactory.GetOrCreate(key);
                               
                                    await Task.Delay(1000);
                                    await jivoSiteService.SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId, "Пару секунд, AI готовит ответ...");

                                    string assistantResponse = await _assistant.GetResponseAsync(clientToBotMessage.Text,
                                        "",
                                        Consts.OpenAIAssistantID_StartSpeaking, clientToBotMessage.ChatId);

                                    await jivoSiteService.SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId, assistantResponse);
                                    await Task.Delay(1000);

                                    // в базе знаний нет
                                    if (assistantResponse.ToLower().Contains("t know the answer"))
                                    {
                                        await jivoSiteService.SendMessageWithButtonsAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                                            "Sorry, I don't know the answer. I can forward it right now to Sasha and he will reply you in 24 hours. You can also fill my standard application form or just ask another question.", "text",
                                            new List<Button>() {
                                                new Button() { Text = "Send question to Sasha", Id = 1 },
                                                new Button() { Text = "Fill application form", Id = 10 },
                                            });
                                    }
                                    else
                                    {

                                    await jivoSiteService.SendMessageWithButtonsAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                                        "Is this the asnwer  you were searching for? If not - I can forward it right now to Sasha and he will reply you in 24 hours. You can also fill my standard application form or just ask another question.", "text",
                                        new List<Button>() {
                                                new Button() { Text = "Send question to Sasha", Id = 1 },
                                                new Button() { Text = "Fill application form", Id = 10 },
                                        });
                                }
                            }
                        ));
                    break;
                case "datacol":
                    stateCollection.AddState(
                        // Forwarding question to the support
                        new BotState(
                            async (clientToBotMessage, chatHistory) => clientToBotMessage.ButtonId == 1
                            ,
                            async (clientToBotMessage, chatHistory) => {
                                var lastTextMessage = chatHistory.GetLastMessage(false)?.Text ?? "NO LAST MESSAGE";
                                var jivoSiteService = _jivoSiteServiceFactory.GetOrCreate(key);
                                await _emailSenderServiceFactory.GetOrCreate(key).SendEmailAsync("isolar2005@gmail.com",
                                    $"{key} chat question forwarded", lastTextMessage);

                            }
                        ));
                   
                    stateCollection.AddState(
                        // Button "Describe parsing task" handler
                        new BotState(
                            async (clientToBotMessage, chatHistory) => clientToBotMessage.ButtonId == 10
                            ,
                            async (clientToBotMessage, chatHistory) => {
                                await _jivoSiteServiceFactory.GetOrCreate(key).SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId, "Какой сайт вы хотите парсить?");
                               
                            }
                        ));
                    stateCollection.AddState(
                        // Scenario "Describing parsing task" 1st step (Site)
                        new BotState(
                            async (clientToBotMessage, chatHistory) =>
                            {
                                var lastMessage = chatHistory.GetLastMessage();
                                if (lastMessage == null) return false;
                                return lastMessage.ButtonId == 10;
                            }
                            ,
                            async (clientToBotMessage, chatHistory) => {
                                clientToBotMessage.SpecialId = "10.1:site_input";
                                await _jivoSiteServiceFactory.GetOrCreate(key).SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                                "Прекрасно, скажите, какие данные нужно собрать?");                            
                            }
                        ));
                    stateCollection.AddState(
                        // Scenario "Describing parsing task" 2nd step (data)
                        new BotState(
                            async (clientToBotMessage, chatHistory) =>
                            {
                                var lastMessage = chatHistory.GetLastMessage();
                                if (lastMessage == null) return false;
                                return lastMessage.SpecialId == "10.1:site_input";
                            }
                            ,
                            async (clientToBotMessage, chatHistory) => {
                                await _emailSenderServiceFactory.GetOrCreate(key).SendEmailAsync("isolar2005@gmail.com",
                                    $"{key} chat task for parsing sent (just last step for now)", clientToBotMessage.Text);
                                await _jivoSiteServiceFactory.GetOrCreate(key).SendMessageWithButtonsAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                                 "Отлично! Отправили вашу задачу нашим разработчикам. В течение суток вы получите оценку стоимости. Есть ли у вас еще вопросы? Также, возможно, вы сразу хотите: ", "text",
                                 new List<Button>() {
                                        new Button() { Text = "Описать еще одну задачу по парсингу", Id = 10 },
                                        new Button() { Text = "Купить программу", Id = 50 }
                                 });
                            }
                        ));
                    stateCollection.AddState(
                        // BUtton "Show similar knowledgebase links" handler
                        new BotState(
                            async (clientToBotMessage, chatHistory) => clientToBotMessage.ButtonId == 20
                            ,
                            async (clientToBotMessage, chatHistory) => {
                                var jivoSiteService = _jivoSiteServiceFactory.GetOrCreate(key);
                                await jivoSiteService.SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId, "Пару секунд, AI готовит ответ...");
                                var lastMessage = chatHistory.GetLastMessage(true);
                                if(lastMessage == null)
                                {
                                    await jivoSiteService.SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                                        "Ошибка: нет данных о прошлом сообщении");
                                }
                                else if (lastMessage.FaqItems == null)
                                {
                                    await jivoSiteService.SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                                        "Ошибка: нет данных о ссылка для прошлого вопроса");
                                }
                                else 
                                {
                                    await jivoSiteService.SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId, lastMessage.FaqItems.BuildFAQReference());
                                }

                                await jivoSiteService.SendMessageWithButtonsAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                                 "Есть ли у вас еще вопросы? Также, возможно, вы хотите: ", "text",
                                    new List<Button>() {
                                    new Button() { Text = "Описать задачу по парсингу", Id = 10 },
                                     // new Button() { Text = "Купить программу", Id = 50 }
                                 });
                            }
                        ));
                    stateCollection.AddState(
                        // Button "Buy" handler
                        new BotState(
                            async (clientToBotMessage, chatHistory) => clientToBotMessage.ButtonId == 50
                            ,
                            async (clientToBotMessage, chatHistory) => {
                                // await _jivoSiteServiceFactory.GetOrCreate(key).SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId, "Какой сайт вы хотите парсить?");
                                await _jivoSiteServiceFactory.GetOrCreate(key).SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                                    "Перейдите, пожалуйста [по ссылке](https://web-data-extractor.net/buy/). Есть ли у вас еще вопросы?");
                            }
                        ));
                    stateCollection.AddState(
                        // Other messages
                        new BotState(
                            async (clientToBotMessage, chatHistory) => true
                            ,
                            async (clientToBotMessage, chatHistory) => {
                                var jivoSiteService = _jivoSiteServiceFactory.GetOrCreate(key);
                                // Check if specific message hello
                                var messageFeatures = await _messageEvaluator.EvaluateMessageFeatures(clientToBotMessage.Text);
                                // Critical
                                if (messageFeatures.Critical)
                                {
                                    await jivoSiteService.SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                                        "Данный вопрос относится к срочным, поэтому передаю его сразу в поддержку! Наша команда свяжется с вами в течение 24 часов. Есть ли у вас еще вопросы?");
                                    await _emailSenderServiceFactory.GetOrCreate(key).SendEmailAsync("isolar2005@gmail.com",
                                        $"{key} Sensitive from client", clientToBotMessage.Text);
                                  
                                }
                                // Just hello
                                else if (!messageFeatures.Complex)
                                {
                                   
                                    await jivoSiteService.SendMessageWithButtonsAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                                     "Я AI помощник Datacol. Пожалуйста, задайте свой вопрос. Также, возможно, вы сразу хотите: ", "text",
                                     new List<Button>() {
                                                    new Button() { Text = "Описать задачу по парсингу", Id = 10 },
                                                    new Button() { Text = "Купить программу", Id = 50 }
                                     });
                                }
                                else
                                {
                                    await Task.Delay(1000);
                                    await jivoSiteService.SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId, "Пару секунд, AI готовит ответ...");

                                    List<FAQItem> faqLinks = await _embeddingManager.GetResponseAsync(clientToBotMessage.Text);
                                    clientToBotMessage.FaqItems = faqLinks;
                                    //await _assistant.GetResponseAsync(clientMessage.Message.Text,"", Consts.OpenAIAssistantID_DC, clientMessage.ChatId);

                                    string assistantResponse = await _assistant.GetResponseAsync(clientToBotMessage.Text,
                                        faqLinks.BuildAssistantInstruction(),
                                        Consts.OpenAIAssistantID_DC, clientToBotMessage.ChatId);

                                    await jivoSiteService.SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId, assistantResponse);
                                    await Task.Delay(1000);

                                    // в базе знаний нет
                                    if (assistantResponse.ToLower().Contains("в базе знаний нет"))
                                    {
                                        await jivoSiteService.SendMessageWithButtonsAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                                            "Могу переслать вопрос на email поддержки. Также, вы можете сформулировать вопрос по-другому.", "text",
                                            new List<Button>() {
                                                new Button() { Text = "Переслать поддержке", Id = 1 },
                                            });
                                    }
                                    else
                                    {

                                        await jivoSiteService.SendMessageWithButtonsAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                                            "Удалось ли найти ответ? Если нет, то могу переслать вопрос на email поддержки или показать похожие статьи из базы знаний. Также, вы можете сформулировать вопрос по-другому либо сразу описать свою задачу по парсингу.", "text",
                                            new List<Button>() {
                                                new Button() { Text = "Переслать поддержке", Id = 1 },
                                                new Button() { Text = "Показать похожие", Id = 20 },
                                                new Button() { Text = "Описать задачу по парсингу", Id = 10 }
                                            });
                                    }
                                }

                            }
                        ));
                    break;
                default:
                    throw new Exception($"No state collection for {key}");
                    break;
            }

            //stateCollection = key switch
            //{
            //    "datacol" => ,
            //    // "startspeaking" => "START_SPEAK",
            //    _ => throw new Exception($"No state collection for {key}")  // Default case
            //};

            return new BotStateManager(stateCollection,_chatDataStorageService);
        }
    }
}
