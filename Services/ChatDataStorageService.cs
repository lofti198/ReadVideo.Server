using System.Collections.Concurrent; // Add this for ConcurrentDictionary
using Microsoft.AspNetCore.SignalR.Protocol;
using ReadVideo.Server.Data;
using ReadVideo.Server.Services.Embeddings.Storage;

namespace ReadVideo.Server.Services
{
    public class ChatData
    {
        public ChatHistory History { get; set; }
        public HashSet<int> ButtonsClicked { get; set; } = new HashSet<int>();

        public ChatData(ChatHistory history)
        {
            History = history;
        }
        public ChatData()
        {
            History = new ChatHistory();
        }

        public void AddMessage(ClientToBotMessage clientToBotMessage)
        {
            History.AddMessage(clientToBotMessage);
        }

        public ClientToBotMessage GetLastMessage(bool anyMessage = true)
        {
            return History.GetLastMessage(anyMessage);
        }

        public ClientToBotMessage GetMessageWithSpecialId(string id)
        {
            return History.GetMessageWithSpecialId(id);
        }
    }
    public class ChatDataStorageService : IChatDataStorageService
    {
        // Replace Dictionary with ConcurrentDictionary
        private readonly ConcurrentDictionary<(string ClientId, string ChatId), ChatData> _chats = new();

        public void SaveData(string clientId, string chatId, ClientToBotMessage message)
        {
            var key = (ClientId: clientId, ChatId: chatId);

            // Use ConcurrentDictionary's AddOrUpdate method for thread-safe write
            _chats.AddOrUpdate(key, new ChatData(new ChatHistory(message)), (key, oldValue) =>
            {
                oldValue.History.AddMessage(message);
                return oldValue;
            });
        }

        public ChatData GetChatData(string clientId, string chatId)
        {
            var key = (ClientId: clientId, ChatId: chatId);
            // Use TryGetValue for thread-safe read, no change needed here as ConcurrentDictionary supports this method
            // _chats.TryGetValue(key, out var chatData);
            return _chats.GetOrAdd(key, _ => new ChatData());
            // return chatData;
        }
    }

    public interface IChatDataStorageService
    {
        void SaveData(string clientId, string chatId, ClientToBotMessage message);
        ChatData GetChatData(string clientId, string chatId);
    }
}
