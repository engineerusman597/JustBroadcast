using JustBroadcast.Models;

namespace JustBroadcast.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task LogoutAsync();
        Task<User?> GetCurrentUserAsync();
        Task<bool> IsAuthenticatedAsync();
    }
}
