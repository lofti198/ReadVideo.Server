using ReadVideo.Server.Data;
using ReadVideo.Server.Services.Embeddings.Storage;

namespace ReadVideo.Server.Services
{

    public class ChatDataStorageService : IChatDataStorageService
    {
        private readonly Dictionary<(string ClientId, string ChatId), ChatData> _messages = new();

        public void SaveData(string clientId, string chatId, ChatData chatData)
        {
            var key = (ClientId: clientId, ChatId: chatId);
            _messages[key] = chatData;// new ChatData { UserRequest = userRequest, FaqItems = faqItems };
        }

        public ChatData GetLastData(string clientId, string chatId)
        {
            var key = (ClientId: clientId, ChatId: chatId);
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
