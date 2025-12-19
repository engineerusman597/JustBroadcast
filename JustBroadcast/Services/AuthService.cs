using Blazored.LocalStorage;
using JustBroadcast.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

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
                // Get API endpoint from configuration
                var apiUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7000";
                var loginEndpoint = _configuration["ApiSettings:LoginEndpoint"] ?? "/api/auth/login";

                var response = await _httpClient.PostAsJsonAsync($"{apiUrl}{loginEndpoint}", request);

                if (response.IsSuccessStatusCode)
                {
                    var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();

                    if (loginResponse != null)
                    {
                        await _localStorage.SetItemAsync("authToken", loginResponse.Token);
                        await _localStorage.SetItemAsync("username", loginResponse.Username);
                        await _localStorage.SetItemAsync("email", loginResponse.Email);
                        await _localStorage.SetItemAsync("role", loginResponse.Role.ToString());

                        // Notify authentication state changed
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
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("username");
            await _localStorage.RemoveItemAsync("email");
            await _localStorage.RemoveItemAsync("role");

            ((CustomAuthStateProvider)_authStateProvider).NotifyUserLogout();
        }

        public async Task<User?> GetCurrentUserAsync()
        {
            try
            {
                var username = await _localStorage.GetItemAsync<string>("username");
                var email = await _localStorage.GetItemAsync<string>("email");
                var roleString = await _localStorage.GetItemAsync<string>("role");

                if (string.IsNullOrEmpty(username))
                    return null;

                return new User
                {
                    Username = username,
                    Email = email ?? string.Empty,
                    Role = Enum.TryParse<UserRole>(roleString, out var role) ? role : UserRole.Viewer
                };
            }
            catch
            {
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
