namespace Backend_Pixel_Crawler.Interface
{
    public interface ITokenCacheService
    {
        Task SetTokenAsync(string token, Guid userId, TimeSpan expiry);
        Task<string> GetUserIdByTokenAsync(string token);
        Task RemoveTokenAsync(string token);
    }
}
