
using HigLabo.OpenAI;
using ReadVideo.Server.Data;
using ReadVideo.Server.Services.AIAssistants;
using ReadVideo.Server.Services.Embeddings.Generation;
using ReadVideo.Server.Services.Embeddings.Storage;
using System.Collections.Concurrent;
using System.Text;

namespace ReadVideo.Server.Services.Embeddings
{
    public class EmbeddingManager : IEmbeddingManager
    {
        private readonly IEmbeddingGenerator _embeddingGenerator;
        private readonly IEmbeddingStorageService _embeddingStorageService;
        public EmbeddingManager(IEmbeddingGenerator embeddingGenerator,
            IEmbeddingStorageService embeddingStorageService)
        {
            _embeddingGenerator = embeddingGenerator;
            _embeddingStorageService = embeddingStorageService;
        }
        public async Task<List<FAQItem>> GetResponseAsync(string userInput)
        {
            var vector = await _embeddingGenerator.GetEmbedding(userInput);
            var faqItems = await _embeddingStorageService.GetRowsByVector(vector);
            return faqItems;

            //StringBuilder decoratedInput = new StringBuilder();


            //foreach (var faqItem in faqItems)
            //{
            //    //decoratedInput.Append($"Вопрос: {faqItem.Question}{Environment.NewLine}"+
            //    //    $"Ответ: {faqItem.Reply}{Environment.NewLine}" +
            //    //    $"Ссылка: {faqItem.Url}{Environment.NewLine}");
            //    decoratedInput.Append($"{faqItem.Question}{Environment.NewLine}" +
            //        // $"Ответ: {faqItem.Reply}{Environment.NewLine}" +
            //        $"{faqItem.Url}{Environment.NewLine}");
            //}
            //return decoratedInput.ToString();
            //// return await _decoratedAssistant.GetResponseAsync(decoratedInput.ToString(), assistantId, threadId);
        }
    }

    public interface IEmbeddingManager
    {
        Task<List<FAQItem>> GetResponseAsync(string userInput);
    }
}
