using Newtonsoft.Json.Linq;
using ReadVideo.Server.Data;
using ReadVideo.Server.Services.AIAssistants;
using ReadVideo.Server.Services.EmailSending;
using ReadVideo.Server.Services.Embeddings;
using ReadVideo.Server.Utils;

namespace ReadVideo.Server.Services.BotStateManagement.Factory
{
    public class StartSpeakingBotStateManagerFactory : BotStateManagerFactoryBase
    {
        const int _fillApplicationButtonId = 10;  
        public StartSpeakingBotStateManagerFactory(DITypeFactoryBase<string, IJivoSiteService> jivoSiteServiceFactory, DITypeFactoryBase<string, IEmailSender> emailSenderServiceFactory, IAssistant assistant, IEmbeddingManager embeddingManager, IMessageEvaluator messageEvaluator, IChatDataStorageService chatDataStorageService)
            : base("startspeaking", jivoSiteServiceFactory, emailSenderServiceFactory, assistant, embeddingManager, messageEvaluator, chatDataStorageService)
        {

        }

        public override BotStateManager Create()
        {
            BotStateCollection stateCollection = new BotStateCollection();

            stateCollection.AddState(
                // Button "Fill applicatoin" handler
                new BotState(
                    async (clientToBotMessage, chatData) => clientToBotMessage.ButtonId == _fillApplicationButtonId
                    ,
                    async (clientToBotMessage, chatHchatDataistory) =>
                    {
                        await _jivoSiteServiceFactory.GetOrCreate(key).SendMessageAsync
                            (clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                            "Шаг 1/3: Сколько Вам лет? Кто Вы по профессии?");

                    }
                ));
            stateCollection.AddState(
                // Scenario "Describing parsing task" 1st step
                new BotState(
                    async (clientToBotMessage, chatData) =>
                    {
                        var lastMessage = chatData.GetLastMessage();
                        if (lastMessage == null) return false;
                        return lastMessage.ButtonId == _fillApplicationButtonId;
                    }
                    ,
                    async (clientToBotMessage, chatData) =>
                    {
                        clientToBotMessage.SpecialId = $"{_fillApplicationButtonId}.1";
                        await _jivoSiteServiceFactory.GetOrCreate(key).SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                        "Шаг 2/3: Отлично! Какой Ваш уровень английского?");
                    }
                ));
            stateCollection.AddState(
                // Scenario "Describing parsing task" 2nd step
                new BotState(
                    async (clientToBotMessage, chatData) =>
                    {
                        var lastMessage = chatData.GetLastMessage();
                        if (lastMessage == null) return false;
                        return lastMessage.SpecialId == $"{_fillApplicationButtonId}.1";
                    }
                    ,
                    async (clientToBotMessage, chatHistory) =>
                    {
                        clientToBotMessage.SpecialId = $"{_fillApplicationButtonId}.2";
                        await _jivoSiteServiceFactory.GetOrCreate(key).SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                        "Шаг 3/3: Наконец - какая Ваша цель изучения English?");
                    }
                ));

            stateCollection.AddState(
                // Scenario "Describing parsing task" 2nd step (data)
                new BotState(
                    async (clientToBotMessage, chatData) =>
                    {
                        var lastMessage = chatData.GetLastMessage();
                        if (lastMessage == null) return false;
                        return lastMessage.SpecialId == $"{_fillApplicationButtonId}.2";
                    }
                    ,
                    async (clientToBotMessage, chatData) =>
                    {
                        // gather complete the request data!!!
                        var copmleteRequestData = Utils.ParticularBotUtils.StartSpeakingBotUtils.GetCopmleteRequestData(clientToBotMessage, chatData);
                        await _emailSenderServiceFactory.GetOrCreate(key).SendEmailAsync("isolar2005@gmail.com",
                            $"{key} chat application for StartSpeaking sent (just last step for now)", copmleteRequestData);
                        await _jivoSiteServiceFactory.GetOrCreate(key).SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                         "Супер! Спасибо, за Ваше время. Саша ответит Вам в течение 24 часов. Если у Вас есть еще вопросы - пожалуйста, задайте их мне.");

                    }
                ));
            stateCollection.AddState(

                // Other messages
                new BotState(
                    async (clientToBotMessage, chatData) => true
                    ,
                    async (clientToBotMessage, chatData) =>
                    {
                        var jivoSiteService = _jivoSiteServiceFactory.GetOrCreate(key);

                        await Task.Delay(500);
                        await jivoSiteService.SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId, "Пару секунд, AI готовит ответ...");

                        string assistantResponse = await _assistant.GetResponseAsync(clientToBotMessage.Text,
                            "",
                            Consts.OpenAIAssistantID_StartSpeaking, clientToBotMessage.ChatId);


                        var buttons = new List<Button>
                        {
                            new Button { Text = "Переслать вопрос Саше", Id = 1 }
                        };

                        if (!chatData.ButtonsClicked.Contains(_fillApplicationButtonId))
                        {
                            buttons.Add(new Button { Text = "Заполнить заявку на обучение", Id = _fillApplicationButtonId });
                        }


                        await jivoSiteService.SendMessageWithButtonsAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId,
                            assistantResponse,
                            assistantResponse,
                            buttons);
                    }
                ));




            return new BotStateManager(stateCollection, _chatDataStorageService);
        }
    }
}
