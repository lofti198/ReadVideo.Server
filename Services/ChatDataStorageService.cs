namespace ReadVideo.Server.Services
{

    public class ChatDataStorageService : IChatDataStorageService
    {
        private readonly Dictionary<(string ClientId, string ChatId), string> _messages = new();

        public void SaveData(string clientId, string chatId, string message)
        {
            var key = (ClientId: clientId, ChatId: chatId);
            _messages[key] = message;
        }

        public string GetLastData(string clientId, string chatId)
        {
            var key = (ClientId: clientId, ChatId: chatId);
            _messages.TryGetValue(key, out var message);
            return message;
        }
    }

    public interface IChatDataStorageService
    {
        void SaveData(string clientId, string chatId, string message);

        string GetLastData(string clientId, string chatId);
    }
}
