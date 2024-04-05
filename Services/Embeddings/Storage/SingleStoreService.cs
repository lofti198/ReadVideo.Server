using Newtonsoft.Json;
using SingleStoreConnector;
using System.Data;

namespace ReadVideo.Server.Services.Embeddings.Storage
{

    public struct FAQItem
    {
        public string Question { get; set; }
        public string Reply { get; set; }
        public string Url { get; set; }
        public double Score { get; set; }
    }

    public class SingleStoreService : IDisposable, IEmbeddingStorageService
    {
        SingleStoreConnection _connection = null;
        public SingleStoreService(string connectionStr)
        {
            _connection = new SingleStoreConnection(connectionStr);
            _connection.Open();
        }


        // Update the return type to be a list of FAQItem
        public async Task<List<FAQItem>> GetRowsByVector(float[] vector)
        {
            // Initialize the list to store the results
            List<FAQItem> items = new List<FAQItem>();

            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                // Convert the vector to JSON array string
                string vectorJson = JsonConvert.SerializeObject(vector);

                string sql = "SELECT question, reply, url, dot_product(embedding, JSON_ARRAY_PACK(@jsonArray)) AS score FROM faq_vectores ORDER BY score DESC LIMIT 5;";

                using var command = new SingleStoreCommand(sql, _connection);
                command.Parameters.AddWithValue("@jsonArray", vectorJson);

                using var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                {
                    // Create a new FAQItem for each row and add it to the list
                    FAQItem item = new FAQItem
                    {
                        Question = reader["question"].ToString(),
                        Reply = reader["reply"].ToString(),
                        Url = reader["url"].ToString(),
                        Score = Convert.ToDouble(reader["score"])
                    };

                    items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }

            return items;
        }

        public void Dispose()
        {
            _connection.Close();
        }
    }
}
