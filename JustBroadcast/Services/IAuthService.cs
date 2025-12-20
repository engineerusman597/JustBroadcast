using JustBroadcast.Models;

namespace JustBroadcast.Services
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest request);
        Task<LoginResponse?> RefreshTokenAsync();
        Task LogoutAsync();
        Task<UserInfoDto?> GetCurrentUserAsync();
        Task<bool> IsAuthenticatedAsync();
    }
}
