using System.Net.Http;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ReadVideo.Server.Services.Embeddings.Generation
{
    public class OpenAIEmbeddingGenerator : IEmbeddingGenerator
    {
        private readonly string _apiKey;
        private readonly string _baseUrl = "https://api.openai.com/v1";
        private readonly string _modelId = "text-embedding-ada-002";

        public OpenAIEmbeddingGenerator(string apiKey)
        {
            _apiKey = apiKey;
        }

        static float[] ExtractEmbedding(string jsonString)
        {
            JObject jsonObject = JObject.Parse(jsonString);

            // Extracting embedding array
            JArray dataArray = (JArray)jsonObject["data"];
            JArray embeddingArray = (JArray)dataArray[0]["embedding"]; // Assuming there's only one item in "data"

            // Convert JArray to float array
            float[] embedding = embeddingArray.Select(token => (float)token).ToArray();

            return embedding;
        }

        public async Task<float[]> GetEmbedding(string text)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

            var requestBody = new
            {
                model = _modelId,
                input = text,
                encoding_format = "float"
            };

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{_baseUrl}/embeddings", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return ExtractEmbedding(responseString);
            }
            else
            {
                throw new Exception($"Error getting embedding: {await response.Content.ReadAsStringAsync()}");
            }
        }

    }
}