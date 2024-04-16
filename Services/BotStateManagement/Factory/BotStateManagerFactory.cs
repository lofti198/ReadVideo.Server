using Newtonsoft.Json.Linq;
using ReadVideo.Server.Data;
using ReadVideo.Server.Services.AIAssistants;
using ReadVideo.Server.Services.EmailSending;
using ReadVideo.Server.Services.Embeddings;
using ReadVideo.Server.Utils;

namespace ReadVideo.Server.Services.BotStateManagement.Factory
{
    public abstract class BotStateManagerFactoryBase
    {
        protected readonly DITypeFactoryBase<string, IJivoSiteService> _jivoSiteServiceFactory;
        protected readonly DITypeFactoryBase<string, IEmailSender> _emailSenderServiceFactory;
        protected readonly IAssistant _assistant; // Service to interact with OpenAI API
        protected readonly IEmbeddingManager _embeddingManager;
        protected readonly IMessageEvaluator _messageEvaluator;
        // private readonly TokenToServiceKeyConverter _tokenToServiceKeyConverter;
        protected readonly IChatDataStorageService _chatDataStorageService;
        protected readonly string key;
        public BotStateManagerFactoryBase(string key, DITypeFactoryBase<string, IJivoSiteService> jivoSiteServiceFactory,
            DITypeFactoryBase<string, IEmailSender> emailSenderServiceFactory, IAssistant assistant,
            IEmbeddingManager embeddingManager, IMessageEvaluator messageEvaluator, IChatDataStorageService chatDataStorageService)
        {
            this.key = key;
            _jivoSiteServiceFactory = jivoSiteServiceFactory;
            _emailSenderServiceFactory = emailSenderServiceFactory;
            _assistant = assistant;
            _embeddingManager = embeddingManager;
            _messageEvaluator = messageEvaluator;
            // _tokenToServiceKeyConverter = tokenToServiceKeyConverter;
            _chatDataStorageService = chatDataStorageService;
        }

        public abstract BotStateManager Create();
       
    }
}
