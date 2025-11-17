using System.ComponentModel.DataAnnotations;

namespace ReadVideo.Server.Models
{
    public class SupportRequestDto
    {
        // VALIDATION DISABLED - Uncomment attributes below to enable validation
        //[Required]
        //[EmailAddress]
        public string Email { get; set; } = string.Empty;

        // VALIDATION DISABLED - Uncomment attributes below to enable validation
        //[Required]
        //[MinLength(3)]
        public string Question { get; set; } = string.Empty;

        public string DatacolXml { get; set; } = string.Empty;

        // VALIDATION DISABLED - Uncomment attribute below to enable validation
        //[Url]
        public string CurrentUrl { get; set; } = string.Empty;
    }
}
