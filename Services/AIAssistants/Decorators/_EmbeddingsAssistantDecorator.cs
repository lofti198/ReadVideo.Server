//using ReadVideo.Server.Services.Embeddings.Generation;
//using ReadVideo.Server.Services.Embeddings.Storage;
//using System.Text;

//namespace ReadVideo.Server.Services.AIAssistants.Decorators
//{
//    public class _EmbeddingsAssistantDecorator : AssistantDecoratorBase
//    {
//        private readonly IEmbeddingGenerator _embeddingGenerator;
//        private readonly IEmbeddingStorageService _embeddingStorageService;

//        public _EmbeddingsAssistantDecorator(IAssistant decoratedAssistant, IEmbeddingGenerator embeddingGenerator,
//            IEmbeddingStorageService embeddingStorageService) : base(decoratedAssistant)
//        {
//            _embeddingGenerator = embeddingGenerator;
//            _embeddingStorageService = embeddingStorageService;
//        }
//        public override async Task<string> GetResponseAsync(string userInput, string assistantId, string threadId)
//        {
//            var vector = await _embeddingGenerator.GetEmbedding(userInput);
//            var faqItems = await _embeddingStorageService.GetRowsByVector(vector);

//            //StringBuilder decoratedInput = new StringBuilder($"Here is user question: {userInput}{Environment.NewLine}"+
//            //    $"Here are FAQ articles, which could be helpful to build the answer{Environment.NewLine}{Environment.NewLine}");
//            StringBuilder decoratedInput = new StringBuilder();
                
//                //new StringBuilder($"Вот текущий вопрос пользователя: {userInput}{Environment.NewLine}" +
//                //$"Вот статьи из FAQ, которые могут пригодиться для формирования ответа:{Environment.NewLine}{Environment.NewLine}");



//            foreach ( var faqItem in faqItems )
//            {
//                //decoratedInput.Append($"Вопрос: {faqItem.Question}{Environment.NewLine}"+
//                //    $"Ответ: {faqItem.Reply}{Environment.NewLine}" +
//                //    $"Ссылка: {faqItem.Url}{Environment.NewLine}");
//                decoratedInput.Append($"{faqItem.Question}{Environment.NewLine}" +
//                    // $"Ответ: {faqItem.Reply}{Environment.NewLine}" +
//                    $"{faqItem.Url}{Environment.NewLine}");
//            }
//            return decoratedInput.ToString();
//            // return await _decoratedAssistant.GetResponseAsync(decoratedInput.ToString(), assistantId, threadId);
//        }

//    }
//}
