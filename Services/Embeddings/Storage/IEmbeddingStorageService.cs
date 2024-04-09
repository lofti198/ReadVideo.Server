
using ReadVideo.Server.Data;

namespace ReadVideo.Server.Services.Embeddings.Storage
{
    public interface IEmbeddingStorageService
    {
        Task<List<FAQItem>> GetRowsByVector(float[] vector);
    }
}