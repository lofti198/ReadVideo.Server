using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReadVideo.Server.Data;
using ReadVideo.Server.Services.AIAssistants;
using ReadVideo.Server.Services.EmailSending;
using ReadVideo.Server.Services.Embeddings;
using ReadVideo.Server.Utils;
using Telegram.Bot;

namespace ReadVideo.Server.Services.BotStateManagement.Factory.Datacol
{
    public class DatacolBotStateManagerFactory : BotStateManagerFactoryBase
    {
        const int _fillTaskButtonId = 10;
        public DatacolBotStateManagerFactory(DITypeFactoryBase<string, IJivoSiteService> jivoSiteServiceFactory, DITypeFactoryBase<string, IEmailSender> emailSenderServiceFactory, IAssistant assistant, IEmbeddingManager embeddingManager, IMessageEvaluator messageEvaluator, IChatDataStorageService chatDataStorageService)
            : base("datacol", jivoSiteServiceFactory, emailSenderServiceFactory, assistant, embeddingManager, messageEvaluator, chatDataStorageService)
        {

        }

        public override BotStateManager Create()
        {
            BotStateCollection stateCollection = new BotStateCollection();
            stateCollection.AddState(
                // Message size limit check
                new BotState(
                    async (clientToBotMessage, chatData) => clientToBotMessage.Text.Length > Consts.ClientMessageSymbolsLimit
                    ,
                    async (clientToBotMessage, chatData) =>
                    {
                        await _jivoSiteServiceFactory.GetOrCreate(key).SendMessageAsync
                            (clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                            $"Ooops) Ваше сообщение больше {Consts.ClientMessageSymbolsLimit} символов. Я такие еще обрабатывать не умею)");

                    }
                ));
            stateCollection.AddState(
                // Forwarding question to the support
                new BotState(
                    async (clientToBotMessage, chatHistory) => clientToBotMessage.ButtonId == 1
                    ,
                    async (clientToBotMessage, chatHistory) =>
                    {
                        var lastTextMessage = chatHistory.GetLastMessage(false)?.Text ?? "NO LAST MESSAGE";
                        var jivoSiteService = _jivoSiteServiceFactory.GetOrCreate(key);
                        await _emailSenderServiceFactory.GetOrCreate(key).SendEmailAsync("isolar2005@gmail.com",
                            $"{key} chat question forwarded", lastTextMessage);

                    }
                ));

            stateCollection.AddState(
                // Button "Describe parsing task" handler
                new BotState(
                    async (clientToBotMessage, chatHistory) => clientToBotMessage.ButtonId == _fillTaskButtonId
                    ,
                    async (clientToBotMessage, chatHistory) =>
                    {
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
                        return lastMessage.ButtonId == _fillTaskButtonId;
                    }
                    ,
                    async (clientToBotMessage, chatHistory) =>
                    {
                        clientToBotMessage.SpecialId = $"{_fillTaskButtonId}.1:site_input";
                        await _jivoSiteServiceFactory.GetOrCreate(key).SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                        "Прекрасно, скажите, какие данные нужно собрать?");
                    }
                ));
            stateCollection.AddState(
                // Scenario "Describing parsing task" 2nd step (data)
                new BotState(
                    async (clientToBotMessage, chatData) =>
                    {
                        var lastMessage = chatData.GetLastMessage();
                        if (lastMessage == null) return false;
                        return lastMessage.SpecialId == $"{_fillTaskButtonId}.1:site_input";
                    }
                    ,
                    async (clientToBotMessage, chatData) =>
                    {
                        var copmleteTaskData = Utils.ParticularBotUtils.StartSpeakingBotUtils.GetCopmleteRequestData(clientToBotMessage, chatData,
                           _fillTaskButtonId);

                        await _emailSenderServiceFactory.GetOrCreate(key).SendEmailAsync("isolar2005@gmail.com",
                            $"{key} chat task for parsing sent (just last step for now)", copmleteTaskData);
                        await _jivoSiteServiceFactory.GetOrCreate(key).SendMessageWithButtonsAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                         "Отлично! Отправили вашу задачу нашим разработчикам. В течение суток вы получите оценку стоимости. Есть ли у вас еще вопросы? Также, возможно, вы сразу хотите: ", "text",
                         new List<Button>() {
                                        new Button() { Text = "Описать еще одну задачу по парсингу", Id = _fillTaskButtonId },
                                        new Button() { Text = "Купить программу", Id = 50 }
                         });
                    }
                ));
            stateCollection.AddState(
                // BUtton "Show similar knowledgebase links" handler
                new BotState(
                    async (clientToBotMessage, chatData) => clientToBotMessage.ButtonId == 20
                    ,
                    async (clientToBotMessage, chatData) =>
                    {
                        var jivoSiteService = _jivoSiteServiceFactory.GetOrCreate(key);
                        await jivoSiteService.SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId, "Пару секунд, AI готовит ответ...");
                        var lastMessage = chatData.GetLastMessage(true);
                        if (lastMessage == null)
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
                                    new Button() { Text = "Описать задачу по парсингу", Id = _fillTaskButtonId },
                                // new Button() { Text = "Купить программу", Id = 50 }
                         });
                    }
                ));
            stateCollection.AddState(
                // Button "Buy" handler
                new BotState(
                    async (clientToBotMessage, chatHistory) => clientToBotMessage.ButtonId == 50
                    ,
                    async (clientToBotMessage, chatHistory) =>
                    {
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
                    async (clientToBotMessage, chatHistory) =>
                    {
                        var jivoSiteService = _jivoSiteServiceFactory.GetOrCreate(key);

                        //// Critical
                        //if (messageFeatures.Critical)
                        //{
                        //    await jivoSiteService.SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                        //        "Данный вопрос относится к срочным, поэтому передаю его сразу в поддержку! Наша команда свяжется с вами в течение 24 часов. Есть ли у вас еще вопросы?");
                        //    await _emailSenderServiceFactory.GetOrCreate(key).SendEmailAsync("isolar2005@gmail.com",
                        //        $"{key} Sensitive from client", clientToBotMessage.Text);

                        //}
                        
                        await Task.Delay(1000);
                        await jivoSiteService.SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId, "Пару секунд, AI готовит ответ...");
                      
                        List<FAQItem> faqLinks = await _embeddingManager.GetResponseAsync(clientToBotMessage.Text);
                        clientToBotMessage.FaqItems = faqLinks;
                    //await _assistant.GetResponseAsync(clientMessage.Message.Text,"", Consts.OpenAIAssistantID_DC, clientMessage.ChatId);

                    var rawAssistantResponse = await _assistant.GetResponseAsync(clientToBotMessage.Text,
                            faqLinks.BuildAssistantInstruction(),
                            Consts.OpenAIAssistantID_DC, clientToBotMessage.ChatId);
                        var assistantResponse = JsonConvert.DeserializeObject<DCAssistantResponse>(rawAssistantResponse);
                        
                        if (assistantResponse.Welcome)
                        {
                            await jivoSiteService.SendMessageWithButtonsAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                             "Я AI помощник Datacol. Я помогу найти ответ на ваш вопрос. Если не найду - подскажу к кому обратиться😉", "text",
                             new List<Button>() {
                                                        new Button() { Text = "Описать задачу по парсингу", Id = _fillTaskButtonId },
                                                        new Button() { Text = "Купить программу", Id = 50 }
                             });
                        }
                        else if(!assistantResponse.Found)
                        {
                            await jivoSiteService.SendMessageWithButtonsAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                                "Не нашел я ответа 😢. Могу прямо сейчас переслать вопрос поддержке", "text",
                                new List<Button>() {
                                                new Button() { Text = "Переслать поддержке", Id = 1 },
                                });
                        }
                        else if(assistantResponse.Response!=null)
                        {
                            await jivoSiteService.SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId, assistantResponse.Response);
                            await Task.Delay(1000);

                            await jivoSiteService.SendMessageWithButtonsAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                                "Этот ли ответ вы искали? Если нет, то могу переслать поддержке или показать похожие статьи из базы знаний.", "text",
                                new List<Button>() {
                                                new Button() { Text = "Переслать поддержке", Id = 1 },
                                                new Button() { Text = "Показать похожие", Id = 20 },
                                                new Button() { Text = "Описать задачу по парсингу", Id = 10 }
                                });
                        }
                        else
                        {
                            throw new Exception("Unknown error for: "+ clientToBotMessage.Text);
                        }
                    }
                ));

            return new BotStateManager(stateCollection, _chatDataStorageService);
        }
    }
}
