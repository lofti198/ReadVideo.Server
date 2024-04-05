
namespace ReadVideo.Server.Services.Embeddings.Storage
{
    public interface IEmbeddingStorageService
    {
        Task<List<FAQItem>> GetRowsByVector(float[] vector);
    }
}