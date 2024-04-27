using HigLabo.OpenAI;
using Microsoft.AspNetCore.Mvc;

namespace ReadVideo.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranscriptionController : ControllerBase
    {
        private readonly OpenAIClient _client;

        public TranscriptionController(OpenAIClient client)
        {
            _client = client;
        }

        [HttpPost("transcribe")]
        public async Task<IActionResult> Transcribe([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    memoryStream.Position = 0; // Reset the memory stream position to the beginning after copy

                    var p = new AudioTranslationsParameter();
                    p.File.SetFile(file.FileName, memoryStream); // Set file with stream
                    p.Model = "whisper-1";

                    var response = await _client.AudioTranslationsAsync(p);
                    var text = response.GetResponseBodyText();

                    return Ok(new { Transcription = text });
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
