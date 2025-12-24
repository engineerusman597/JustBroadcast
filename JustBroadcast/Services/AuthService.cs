using Blazored.LocalStorage;
using JustBroadcast.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Text.Json;

namespace JustBroadcast.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly IConfiguration _configuration;

        public AuthService(
            HttpClient httpClient,
            ILocalStorageService localStorage,
            AuthenticationStateProvider authStateProvider,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
            _configuration = configuration;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            try
            {
                // Check if mock authentication is enabled
                var useMockAuth = Convert.ToBoolean(_configuration["ApiSettings:UseMockAuth"]);
                if (useMockAuth)
                {
                    return await MockLoginAsync(request);
                }

                // Get API endpoint from configuration
                var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://cracklier-alia-uninserted.ngrok-free.dev";
                var loginEndpoint = _configuration["ApiSettings:LoginEndpoint"] ?? "/api/Auth/login";

                var response = await _httpClient.PostAsJsonAsync($"{apiUrl}{loginEndpoint}", request);

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    if (loginResponse != null && loginResponse.Token != null && loginResponse.User != null)
                    {
                        // Store tokens and user info
                        await _localStorage.SetItemAsync("authToken", loginResponse.Token);
                        await _localStorage.SetItemAsync("refreshToken", loginResponse.RefreshToken);
                        await _localStorage.SetItemAsync("expiresAt", loginResponse.ExpiresAt);
                        await _localStorage.SetItemAsStringAsync("userInfo", JsonSerializer.Serialize(loginResponse.User));

                        // Notify authentication state changed
                        ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(loginResponse.Token);

                        return loginResponse;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<LoginResponse?> MockLoginAsync(LoginRequest request)
        {
            // Create mock user data for development
            var mockUser = new UserInfoDto
            {
                Id = "mock-user-123",
                Username = "admin",
                Name = "Mock Admin User",
                Role = (int)UserRole.Supervisor, // Supervisor role to see all menu items
                Picture = null,
                AllPlayouts = new List<PlayoutListDto>
                {
                    new PlayoutListDto { Id = "playout-1", Name = "Playout 1", Channel = "Channel A", Spare = false },
                    new PlayoutListDto { Id = "playout-2", Name = "Playout 2", Channel = "Channel B", Spare = true },
                    new PlayoutListDto { Id = "playout-3", Name = "Playout 3", Channel = "Channel C", Spare = false }
                },
                RemotePlayouts = new List<PlayoutListDto>
                {
                    new PlayoutListDto { Id = "remote-1", Name = "Remote Playout 1", Channel = "Channel A", Spare = false },
                    new PlayoutListDto { Id = "remote-2", Name = "Remote Playout 2", Channel = "Channel B", Spare = true }
                },
                SchedulerChannels = new List<ChannelListDto>
                {
                    new ChannelListDto { Id = "channel-1", Name = "Channel A" },
                    new ChannelListDto { Id = "channel-2", Name = "Channel B" },
                    new ChannelListDto { Id = "channel-3", Name = "Channel C" }
                }
            };

            var loginResponse = new LoginResponse
            {
                Token = "mock-jwt-token-" + Guid.NewGuid().ToString(),
                RefreshToken = "mock-refresh-token-" + Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                User = mockUser
            };

            // Store tokens and user info
            await _localStorage.SetItemAsync("authToken", loginResponse.Token);
            await _localStorage.SetItemAsync("refreshToken", loginResponse.RefreshToken);
            await _localStorage.SetItemAsync("expiresAt", loginResponse.ExpiresAt);
            await _localStorage.SetItemAsStringAsync("userInfo", JsonSerializer.Serialize(loginResponse.User));

            // Notify authentication state changed
            ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(loginResponse.Token);

            return loginResponse;
        }

        public async Task<LoginResponse?> RefreshTokenAsync()
        {
            try
            {
                var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");
                if (string.IsNullOrEmpty(refreshToken))
                    return null;

                var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://cracklier-alia-uninserted.ngrok-free.dev";
                var refreshEndpoint = _configuration["ApiSettings:RefreshEndpoint"] ?? "/api/Auth/refresh";

                var request = new RefreshTokenRequest { RefreshToken = refreshToken };
                var response = await _httpClient.PostAsJsonAsync($"{apiUrl}{refreshEndpoint}", request);

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    if (loginResponse != null && loginResponse.Token != null)
                    {
                        await _localStorage.SetItemAsync("authToken", loginResponse.Token);
                        await _localStorage.SetItemAsync("refreshToken", loginResponse.RefreshToken);
                        await _localStorage.SetItemAsync("expiresAt", loginResponse.ExpiresAt);
                        if (loginResponse.User != null)
                        {
                            await _localStorage.SetItemAsStringAsync("userInfo", JsonSerializer.Serialize(loginResponse.User));
                        }

                        ((CustomAuthStateProvider)_authStateProvider).NotifyUserAuthentication(loginResponse.Token);

                        return loginResponse;
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");
                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://cracklier-alia-uninserted.ngrok-free.dev";
                    var logoutEndpoint = _configuration["ApiSettings:LogoutEndpoint"] ?? "/api/Auth/logout";

                    var request = new RefreshTokenRequest { RefreshToken = refreshToken };
                    await _httpClient.PostAsJsonAsync($"{apiUrl}{logoutEndpoint}", request);
                }
            }
            catch
            {
                // Ignore errors during logout API call
            }
            finally
            {
                // Clear local storage
                await _localStorage.RemoveItemAsync("authToken");
                await _localStorage.RemoveItemAsync("refreshToken");
                await _localStorage.RemoveItemAsync("expiresAt");
                await _localStorage.RemoveItemAsync("userInfo");

                ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
            }
        }

        public async Task<UserInfoDto?> GetCurrentUserAsync()
        {
            try
            {
                var userInfoJson = await _localStorage.GetItemAsStringAsync("userInfo");
                if (string.IsNullOrEmpty(userInfoJson))
                    return null;

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var userInfo = JsonSerializer.Deserialize<UserInfoDto>(userInfoJson, options);

                // Debug logging
                Console.WriteLine($"GetCurrentUserAsync - UserInfo: {userInfoJson}");
                Console.WriteLine($"GetCurrentUserAsync - Role: {userInfo?.Role}");

                return userInfo;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deserializing user info: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            return !string.IsNullOrEmpty(token);
        }
    }
}
