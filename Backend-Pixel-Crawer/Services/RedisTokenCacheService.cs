using Backend_Pixel_Crawler.Interface;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;

namespace Backend_Pixel_Crawler.Services
{
    public class RedisTokenCacheService : ITokenCacheService
    {
        private readonly IDistributedCache _cache;

        public RedisTokenCacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task SetTokenAsync(string token, Guid userId, TimeSpan expiry)
        {

            var options = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(1));
            await _cache.SetStringAsync(userId.ToString(), token, options);
        }
        public async Task<string> GetTokenAsync(string token)
        {
            return await _cache.GetStringAsync(token);
        }

        public async Task RemoveTokenAsync(string token)
        {
            await _cache.RemoveAsync(token);
        }


        public async Task SetUserEmailAsync(string userEmail, Guid userId, TimeSpan expiry)
        {

            var options = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(1));
            await _cache.SetStringAsync(userId.ToString(), userEmail, options);
        }

        public async Task<string> GetUserEmailAsync(string userId)
        {
            return await _cache.GetStringAsync(userId);
        }

        
    }
}
