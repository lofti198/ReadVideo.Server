using System.Collections.Concurrent; // Add this for ConcurrentDictionary
using ReadVideo.Server.Data;
using ReadVideo.Server.Services.Embeddings.Storage;

namespace ReadVideo.Server.Services
{
    public class ChatDataStorageService : IChatDataStorageService
    {
        // Replace Dictionary with ConcurrentDictionary
        private readonly ConcurrentDictionary<(string ClientId, string ChatId), ChatData> _messages = new();

        public void SaveData(string clientId, string chatId, ChatData chatData)
        {
            var key = (ClientId: clientId, ChatId: chatId);
            // Use ConcurrentDictionary's AddOrUpdate method for thread-safe write
            _messages.AddOrUpdate(key, chatData, (key, oldValue) => chatData);
        }

        public ChatData GetLastData(string clientId, string chatId)
        {
            var key = (ClientId: clientId, ChatId: chatId);
            // Use TryGetValue for thread-safe read, no change needed here as ConcurrentDictionary supports this method
            _messages.TryGetValue(key, out var chatData);
            return chatData;
        }
    }

    public interface IChatDataStorageService
    {
        void SaveData(string clientId, string chatId, ChatData chatData);
        ChatData GetLastData(string clientId, string chatId);
    }
}
