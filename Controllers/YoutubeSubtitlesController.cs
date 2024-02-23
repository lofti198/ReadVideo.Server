using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using ReadVideo.Server.Data;
using ReadVideo.Services.YoutubeManagement;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ReadVideo.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class YoutubeSubtitlesController : ControllerBase
    {
        private readonly IYoutubeSubtitleService _subtitleService;
        private readonly MongoDbContext _dbContext;
        private readonly IMemoryCache _memoryCache;
        public YoutubeSubtitlesController(IYoutubeSubtitleService subtitleService, MongoDbContext dbContext, IMemoryCache memoryCache)
        {
            this._subtitleService = subtitleService;
            this._dbContext = dbContext;
            this._memoryCache = memoryCache;
        }

        // [Authorize]
        [HttpGet("LoadSubtitles")]
        public async Task<IActionResult> LoadSubtitles([FromQuery] string videoId, [FromQuery] string language ="", [FromQuery] bool full = true)
        {
           
                
                Console.WriteLine($"Get subtitles for: {videoId}");
                var subtitles = await _subtitleService.ExtractSubtitle(videoId, language, full);

                if (subtitles == null)
                {
                    return NotFound("Subtitles not found.");
                }

                return Ok(subtitles);

        }

        [HttpGet("LoadTextBlocks")]
        public async Task<IActionResult> LoadTextBlocks([FromQuery] string videoId, [FromQuery] string language = "", [FromQuery] int minspan = 1200)
        {
        
                Console.WriteLine($"Get subtitles for: {videoId}");
                //var subtitles = await _subtitleService.ExtractSubtitle(videoId, language, full);

                //if (subtitles == null)
                //{
                //    return NotFound("Subtitles not found.");
                //}

                //return Ok(subtitles);

                var textBlocks = await _subtitleService.ExtractSubtitleAsTextBlocks(videoId, language, minspan);

                if (textBlocks == null)
                {
                    return NotFound("Subtitles not found.");
                }

                return Ok(textBlocks);
          
        }

        private async Task<bool> CheckUserInMongo(string email)
        {
            // Check if the user exists in the MongoDB Users collection
            var user = await _dbContext.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
            return user != null;
        }

        private async Task AddUserToMongo(string email, string fullname)
        {
            // Add the user to the MongoDB Users collection
            var user = new User { Email = email, Fullname = fullname };
            await _dbContext.Users.InsertOneAsync(user);
        }
    }

}
