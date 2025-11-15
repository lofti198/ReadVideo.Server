using System.ComponentModel.DataAnnotations;

namespace ReadVideo.Server.Models
{
    public class SupportRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(10)]
        public string Question { get; set; } = string.Empty;

        public string DatacolXml { get; set; } = string.Empty;

        [Url]
        public string CurrentUrl { get; set; } = string.Empty;
    }
}
