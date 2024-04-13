using System.Collections.Concurrent; // Add this for ConcurrentDictionary
using ReadVideo.Server.Data;
using ReadVideo.Server.Services.Embeddings.Storage;

namespace ReadVideo.Server.Services
{
    public class ChatDataStorageService : IChatDataStorageService
    {
        // Replace Dictionary with ConcurrentDictionary
        private readonly ConcurrentDictionary<(string ClientId, string ChatId), ChatHistory> _chats = new();

        public void SaveData(string clientId, string chatId, ClientToBotMessage message)
        {
            var key = (ClientId: clientId, ChatId: chatId);

            // Use ConcurrentDictionary's AddOrUpdate method for thread-safe write
            _chats.AddOrUpdate(key, new ChatHistory(message), (key, oldValue) =>
            {
                oldValue.AddMessage(message);
                return oldValue;
            });
        }

        public ChatHistory GetChatData(string clientId, string chatId)
        {
            var key = (ClientId: clientId, ChatId: chatId);
            // Use TryGetValue for thread-safe read, no change needed here as ConcurrentDictionary supports this method
            // _chats.TryGetValue(key, out var chatData);
            return _chats.GetOrAdd(key, _ => new ChatHistory());
            // return chatData;
        }
    }

    public interface IChatDataStorageService
    {
        void SaveData(string clientId, string chatId, ClientToBotMessage message);
        ChatHistory GetChatData(string clientId, string chatId);
    }
}
