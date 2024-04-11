using System.Text;
using System.Text.Json;

namespace ReadVideo.Server.Data
{
    public class ChatData
    {
        public List<ClientToBotMessage> Messages { get; set; } = new List<ClientToBotMessage>();
        // public string PrevUserRequest { get; set; }

        // public int LastButtonId { get;set; }
        public void AddMessage(ClientToBotMessage message)=>Messages.Add(message);

        public ChatData(ClientToBotMessage message)
        {
            AddMessage(message);
        }

        public ClientToBotMessage GetLastMessage()
        {
            // Iterate through the Messages list in reverse
            for (int i = Messages.Count - 1; i >= 0; i--)
            {
                // Check if the ButtonId of the message is 0
                if (Messages[i].ButtonId == 0)
                {
                    // If ButtonId is 0, return this message as it's considered a question
                    return Messages[i];
                }
            }

            // If no message meets the criteria, return null or handle accordingly
            return null;
        }
    }

    public class ClientToBotMessage
    {
        public string SpecialId { get; set; } = "";  
        public string Text { get; set; }

        //TODO: arch - move
        public List<FAQItem> FaqItems { get; set; } = null;
        public int ButtonId { get; set; } = 0;

        public ClientToBotMessage(string text, List<FAQItem> faqItems = null)
        {
            Text = text;
            FaqItems = faqItems;
        }
    }

    // Static class for extension methods
    public static class ListExtensions
    {
        public static string BuildAssistantInstruction(this List<FAQItem> faqItems)
        {
            StringBuilder output = new StringBuilder(//$"Вот текущий вопрос пользователя: {userInput}{Environment.NewLine}" +
            $"Вот статьи из FAQ, которые могут пригодиться для формирования ответа. Если ответ найден в них, то добавляй к нему ссылки на статьи, в которых он найден.{Environment.NewLine}{Environment.NewLine}");

            foreach (var faqItem in faqItems)
            {
                //decoratedInput.Append($"Вопрос: {faqItem.Question}{Environment.NewLine}"+
                //    $"Ответ: {faqItem.Reply}{Environment.NewLine}" +
                //    $"Ссылка: {faqItem.Url}{Environment.NewLine}");
                output.Append($"# Вопрос: {faqItem.Question}{Environment.NewLine}" +
                     $"## Ответ: {faqItem.Reply}{Environment.NewLine}" +
                $"## Ссылка: {faqItem.Url}{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}");
            }
            return output.ToString();
        }
        public static string BuildAssistantInstructionAsJson(this List<FAQItem> faqItems)
        {
            var items = faqItems.Select(faqItem => new
            {
                Question = faqItem.Question,
                Response = faqItem.Reply, // Assuming 'Reply' is the equivalent of 'Response'
                Url = faqItem.Url
            }).ToList();

            string jsonString = JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
            return jsonString;
        }
        public static string BuildFAQReference(this List<FAQItem> faqItems)
        {
            StringBuilder output = new StringBuilder($"Эти материалы из базы знаний могут быть полезны:{Environment.NewLine}{Environment.NewLine}");

            foreach (var faqItem in faqItems)
            {
                output.Append($"[{faqItem.Question}]({faqItem.Url}){Environment.NewLine}{Environment.NewLine}");
            }
            return output.ToString().Trim();
        }
    }
    public struct FAQItem
    {
        public string Question { get; set; }
        public string Reply { get; set; }
        public string Url { get; set; }
        public double Score { get; set; }
    }
}
