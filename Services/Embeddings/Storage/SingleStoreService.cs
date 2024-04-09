//using Newtonsoft.Json;
//using ReadVideo.Server.Data;
//using SingleStoreConnector;
//using System.Data;

//namespace ReadVideo.Server.Services.Embeddings.Storage
//{
//    public class SingleStoreService : IEmbeddingStorageService, IAsyncDisposable
//    {
//        private SingleStoreConnection _connection = null;
//        private readonly string _connectionStr;

//        public SingleStoreService(string connectionStr)
//        {
//            _connectionStr = connectionStr;
//        }

//        public async Task InitializeAsync()
//        {
//            _connection = new SingleStoreConnection(_connectionStr);
//            await _connection.OpenAsync();
//        }

//        public async Task<List<FAQItem>> GetRowsByVector(float[] vector)
//        {
//            List<FAQItem> items = new List<FAQItem>();

//            try
//            {
//                if (_connection.State != ConnectionState.Open)
//                    await _connection.OpenAsync();

//                string vectorJson = JsonConvert.SerializeObject(vector);
//                string sql = "SELECT question, reply, url, dot_product(embedding, JSON_ARRAY_PACK(@jsonArray)) AS score FROM faq_vectores ORDER BY score DESC LIMIT 5;";

//                using var command = new SingleStoreCommand(sql, _connection);
//                command.Parameters.AddWithValue("@jsonArray", vectorJson);

//                using var reader = await command.ExecuteReaderAsync();
//                while (reader.Read())
//                {
//                    FAQItem item = new FAQItem
//                    {
//                        Question = reader["question"].ToString(),
//                        Reply = reader["reply"].ToString(),
//                        Url = reader["url"].ToString(),
//                        Score = Convert.ToDouble(reader["score"])
//                    };

//                    items.Add(item);
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Exception: " + ex.Message);
//            }

//            return items;
//        }

//        public async ValueTask DisposeAsync()
//        {
//            if (_connection != null)
//            {
//                await _connection.CloseAsync();
//                await _connection.DisposeAsync();
//            }
//        }
//    }
//}


using Newtonsoft.Json;
using ReadVideo.Server.Data;
using SingleStoreConnector;
using System.Data;
using System.Text;

namespace ReadVideo.Server.Services.Embeddings.Storage
{


    public class SingleStoreService : IEmbeddingStorageService
    {
        private readonly string _connectionStr;

        public SingleStoreService(string connectionStr)
        {
            _connectionStr = connectionStr;
        }


        public async Task<List<FAQItem>> GetRowsByVector(float[] vector)
        {
            List<FAQItem> items = new List<FAQItem>();

            // Use using statement to ensure the connection is closed and disposed properly even if an exception occurs
            using (var connection = new SingleStoreConnection(_connectionStr))
            {
                await connection.OpenAsync();

                string vectorJson = JsonConvert.SerializeObject(vector);
                string sql = "SELECT question, reply, url, dot_product(embedding, JSON_ARRAY_PACK(@jsonArray)) AS score FROM faq_vectores ORDER BY score DESC LIMIT 5;";

                using (var command = new SingleStoreCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@jsonArray", vectorJson);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (reader.Read())
                        {
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
                }
            } // The connection is automatically closed here when exiting the using block

            return items;
        }
    }

    //public void Dispose()
    //{
    //    _connection.Close();
    //}

}
