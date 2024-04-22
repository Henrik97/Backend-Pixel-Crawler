namespace Backend_Pixel_Crawler.Interface
{
    public interface IUserAuthenticationService
    {
        Task<(bool IsAuthenticated, string Token)> AuthenticateUserAsync(string username, string password);
    }
}
