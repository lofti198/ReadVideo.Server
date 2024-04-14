using ReadVideo.Server.Data;

namespace ReadVideo.Server.Utils.ParticularBotUtils
{
    public static class StartSpeakingBotUtils
    {
        public static string GetCopmleteRequestData(string currentMessageInput, ChatHistory chatHistory)
        {
           
            return $"General data: {chatHistory.GetMessageWithSpecialId("10.1").Text}{Environment.NewLine}"+
                $"Level of English: {chatHistory.GetMessageWithSpecialId("10.2").Text}{Environment.NewLine}" +
                $"Goal: {currentMessageInput}{Environment.NewLine}";
        }
    }
}
