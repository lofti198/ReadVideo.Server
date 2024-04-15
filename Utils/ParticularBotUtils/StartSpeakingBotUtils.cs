using ReadVideo.Server.Data;
using ReadVideo.Server.Services;

namespace ReadVideo.Server.Utils.ParticularBotUtils
{
    public static class StartSpeakingBotUtils
    {
        public static string GetCopmleteRequestData(ClientToBotMessage currentMessage, ChatData chatData)
        {
           
            return $"Name: {currentMessage.ClientName}{Environment.NewLine}" + 
                $"General data: {chatData.GetMessageWithSpecialId("10.1").Text}{Environment.NewLine}"+
                $"Level of English: {chatData.GetMessageWithSpecialId("10.2").Text}{Environment.NewLine}" +
                $"Goal: {currentMessage.Text}{Environment.NewLine}";
        }
    }
}
