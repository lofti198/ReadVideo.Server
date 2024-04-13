using Newtonsoft.Json.Linq;
using ReadVideo.Server.Data;

namespace ReadVideo.Server.Services.BotStateManagement
{
    public class BotState
    {
        public Func<ClientToBotMessage, ChatHistory, Task<bool>> IsApplicable;

        public Func<ClientToBotMessage, ChatHistory,Task> Operation;
        public BotState(Func<ClientToBotMessage, ChatHistory, Task<bool>> isApplicable, Func<ClientToBotMessage, ChatHistory, Task> operation)
        {
            IsApplicable = isApplicable;
            Operation = operation;
        }

        
    }
}
