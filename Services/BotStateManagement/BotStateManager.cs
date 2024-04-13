using ReadVideo.Server.Data;
using System.Runtime.CompilerServices;

namespace ReadVideo.Server.Services.BotStateManagement
{
    public class BotStateManager
    {
        private readonly BotStateCollection _stateCollection;
        private readonly IChatDataStorageService _chatDataStorage;
        public BotStateManager(BotStateCollection stateCollection, IChatDataStorageService chatDataStorage)
        {
            _stateCollection = stateCollection;
            _chatDataStorage = chatDataStorage;
        }

        public async Task FindAppropriateAndExecute(ClientToBotMessage clientToBotMessage)
        {
            ChatHistory chatHistory = _chatDataStorage.GetChatData(clientToBotMessage.ClientId, clientToBotMessage.ChatId);

            foreach (BotState state in _stateCollection)
            {
                if(await state.IsApplicable(clientToBotMessage, chatHistory))
                {
                    await state.Operation(clientToBotMessage, chatHistory);
                    chatHistory.AddMessage(clientToBotMessage);
                    return;
                }
            }
        }
    }
}
