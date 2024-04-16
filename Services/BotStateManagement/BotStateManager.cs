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
            ChatData chatData = _chatDataStorage.GetChatData(clientToBotMessage.ClientId, clientToBotMessage.ChatId);
            
            // Save info on buttons already clicked
            chatData.ButtonsClicked.Add(clientToBotMessage.ButtonId);

            foreach (BotState state in _stateCollection)
            {
                if(await state.IsApplicable(clientToBotMessage, chatData))
                {

                    await state.Operation(clientToBotMessage, chatData);
                    chatData.AddMessage(clientToBotMessage);
                    return;
                }
            }
        }
    }
}
