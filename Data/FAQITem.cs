using System.Text;
using System.Text.Json;

namespace ReadVideo.Server.Data
{
    public class ChatData
    {
        public string UserRequest { get; set; }
        public List<FAQItem> FaqItems { get; set; } = new List<FAQItem>();
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
            StringBuilder output = new StringBuilder();

            foreach (var faqItem in faqItems)
            {
                output.Append($"{faqItem.Question}{Environment.NewLine}" +
                $"{faqItem.Url}{Environment.NewLine}");
            }
            return output.ToString();
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
