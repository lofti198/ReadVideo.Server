using Newtonsoft.Json;
using ReadVideo.Server.Data;
using ReadVideo.Server.Services.AIAssistants;
using System.Text.Json.Serialization;

namespace ReadVideo.Server.Services
{
    public class MessageEvaluator : IMessageEvaluator
    {
        private readonly IAssistant _assistant;

        public MessageEvaluator(IAssistant assistant)
        {
            _assistant = assistant;
        }
      
        public async Task<MessageFeatures> EvaluateMessageFeatures(string text)
        {
            string result = await _assistant.GetResponseAsync(text, "",
                Consts.GeneralOpenAIAssistantID_HelloDetector, "0");
            return ParseComplexValue(result);
        }

        private static MessageFeatures ParseComplexValue(string jsonString)
        {
            try
            {
                MessageFeatures parsedObject = JsonConvert.DeserializeObject<MessageFeatures>(jsonString);
                return parsedObject;
            }
            catch (Exception ex)
            {
                // Handle or log parsing errors
                Console.WriteLine("Error parsing JSON: " + ex.Message);
                return null; // Or handle this scenario as needed
            }
        }
      
    }

    public interface IMessageEvaluator
    {
        Task<MessageFeatures> EvaluateMessageFeatures(string text);
    }
    public class MessageFeatures
    {
        [JsonPropertyName("complex")]
        public bool Complex { get; set; }

        [JsonPropertyName("critical")]
        public bool Critical { get; set; }
    }



}
