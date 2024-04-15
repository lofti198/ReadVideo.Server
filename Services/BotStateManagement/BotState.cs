using Newtonsoft.Json.Linq;
using ReadVideo.Server.Data;

namespace ReadVideo.Server.Services.BotStateManagement
{
    public class BotState
    {
        public Func<ClientToBotMessage, ChatData, Task<bool>> IsApplicable;

        public Func<ClientToBotMessage, ChatData, Task> Operation;
        public BotState(Func<ClientToBotMessage, ChatData, Task<bool>> isApplicable, Func<ClientToBotMessage, ChatData, Task> operation)
        {
            IsApplicable = isApplicable;
            Operation = operation;
        }

        
    }
}
