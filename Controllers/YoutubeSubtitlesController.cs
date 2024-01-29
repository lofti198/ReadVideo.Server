using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ReadVideo.Services.YoutubeManagement;
using YoutubeExplode.Videos;

namespace ReadVideo.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YoutubeSubtitlesController : ControllerBase
    {
        private readonly IYoutubeSubtitleService subtitleService;

        public YoutubeSubtitlesController(IYoutubeSubtitleService subtitleService)
        {
            this.subtitleService = subtitleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSubtitles(string videoId, string language = "")
        {
            // Now you can use the videoId and subtitleService as needed
            var subtitles = await subtitleService.ExtractSubtitle(videoId, language);

            if (subtitles == null)
            {
                return NotFound(); // or return an appropriate HTTP status code
            }

            // Do something with the subtitles and return a response
            return Ok(subtitles);

        }

        //[HttpGet]
        //public async Task<IActionResult> GetSubtitles()
        //{
           
        //    // Do something with the subtitles and return a response
        //    return Ok("Test");

        //}
    }
}
