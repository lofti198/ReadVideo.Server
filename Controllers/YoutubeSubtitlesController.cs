using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using ReadVideo.Server.Data;
using ReadVideo.Services.YoutubeManagement;
using System;
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

        [HttpPost("LoadSubtitles")]
        public async Task<IActionResult> LoadSubtitles([FromBody] UserRequest userRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(userRequest.Email))
                {
                    return BadRequest("Email cannot be empty.");
                }

                // Check if user exists in the server cache

                // Check if the user with the given email is in the cache
                if (!_memoryCache.TryGetValue(userRequest.Email, out string cachedFullname))
                {
                    // If not in cache, add to cache
                    var cacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) // You can adjust the expiration time as needed
                    };

                    // Store user information in cache
                    _memoryCache.Set(userRequest.Email, userRequest.Fullname, cacheEntryOptions);

                    // Now you can proceed with other operations (e.g., checking MongoDB, etc.)
                    // Check if user exists in the MongoDB Users collection
                    var userExistsInMongo = await CheckUserInMongo(userRequest.Email);

                    if (!userExistsInMongo)
                    {
                        // Add user to the MongoDB Users collection
                        await AddUserToMongo(userRequest.Email, userRequest.Fullname);
                    }
                }
                

                Console.WriteLine($"get subtitles for: {userRequest.VideoId}");
                var subtitles = await _subtitleService.ExtractSubtitle(userRequest.VideoId, userRequest.Language);

                if (subtitles == null)
                {
                    return NotFound(); // or return an appropriate HTTP status code
                }

                // Do something with the subtitles and return a response
                return Ok(subtitles);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        private bool CheckUserInCache(string email, string fullname)
        {
            // Implement your logic to check if the user exists in the server cache
            // Return true if user exists, false otherwise
            return false;
        }

        private void AddUserToCache(string email, string fullname)
        {
            // Implement your logic to add the user to the server cache
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

    public class UserRequest
    {
        public string Email { get; set; }
        public string Fullname { get; set; }

        public string VideoId { get; set; }
        public string Language { get; set; }
    }
}
