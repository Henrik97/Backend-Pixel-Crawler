namespace Backend_Pixel_Crawler.Interface
{
    public interface ITokenCacheService
    {
        Task SetTokenAsync(string token, Guid userId, TimeSpan expiry);
        Task<string> GetTokenAsync(string token);
        Task RemoveTokenAsync(string token);

        Task SetUserEmailAsync(string userEmail, Guid userId, TimeSpan expiry);
        Task<string> GetUserEmailAsync(string userEmail);
    }
}
