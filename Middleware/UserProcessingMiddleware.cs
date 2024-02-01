using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;
using ReadVideo.Server.Data;

namespace ReadVideo.Server.Middleware
{
    public class UserProcessingMiddleware
    {
        private readonly RequestDelegate _next;
        private IMemoryCache _memoryCache;
        private MongoDbContext _dbContext; // Replace YourDbContextType with your actual DbContext type

        public UserProcessingMiddleware(RequestDelegate next, IMemoryCache memoryCache, MongoDbContext dbContext)
        {
            _next = next;
            _memoryCache = memoryCache;
            _dbContext = dbContext;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string email = context.Request.Headers["email"].FirstOrDefault();
            string fullname = context.Request.Headers["name"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(fullname))
            {
                if (!_memoryCache.TryGetValue(email, out string cachedFullname))
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                    };

                    _memoryCache.Set(email, fullname, cacheEntryOptions);

                    if (await CheckUserInMongo(email) == false)
                    {
                        await AddUserToMongo(email, fullname);
                    }
                }
            }

            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }

        private async Task<bool> CheckUserInMongo(string email)
        {
            var user = await _dbContext.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
            return user != null;
        }

        private async Task AddUserToMongo(string email, string fullname)
        {
            var user = new User { Email = email, Fullname = fullname };
            await _dbContext.Users.InsertOneAsync(user);
        }
    }
                                                          
}
