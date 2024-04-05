namespace ReadVideo.Server.Services.Embeddings.Generation
{
    public interface IEmbeddingGenerator
    {
        Task<float[]> GetEmbedding(string text);
    }
}