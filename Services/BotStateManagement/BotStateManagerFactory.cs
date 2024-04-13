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
                case "datacol":
                    stateCollection.AddState(new BotState(
                        async (clientToBotMessage, chatHistory) => clientToBotMessage.ButtonId == 1
                        ,
                        async (clientToBotMessage, chatHistory) => {
                            
                            var jivoSiteService = _jivoSiteServiceFactory.GetOrCreate(key);

                            await jivoSiteService.InviteAgentAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId);
                        }
                    ));
                   
                    stateCollection.AddState(new BotState(
                            async (clientToBotMessage, chatHistory) => clientToBotMessage.ButtonId == 10
                            ,
                            async (clientToBotMessage, chatHistory) => {

                                await _jivoSiteServiceFactory.GetOrCreate(key).SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId, "Какой сайт вы хотите парсить?");
                               
                            }
                        ));
                    stateCollection.AddState(new BotState(
                            async (clientToBotMessage, chatHistory) => true
                            ,
                            async (clientToBotMessage, chatHistory) => {
                             
                                await _jivoSiteServiceFactory.GetOrCreate(key).SendMessageAsync(clientToBotMessage.ClientId, clientToBotMessage.ChatId, "Default case");

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
