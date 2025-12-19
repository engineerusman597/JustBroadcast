using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace JustBroadcast.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _httpClient;

        public CustomAuthStateProvider(ILocalStorageService localStorage, HttpClient httpClient)
        {
            _localStorage = localStorage;
            _httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrEmpty(token))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            // Set authorization header
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return new AuthenticationState(user);
        }

        public void NotifyUserAuthentication(string token)
        {
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            // Set authorization header
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public void NotifyUserLogout()
        {
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);

            // Clear authorization header
            _httpClient.DefaultRequestHeaders.Authorization = null;

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            var handler = new JwtSecurityTokenHandler();

            try
            {
                var token = handler.ReadJwtToken(jwt);

                // Add all claims from the token
                claims.AddRange(token.Claims);

                // Ensure we have a name claim
                if (!claims.Any(c => c.Type == ClaimTypes.Name))
                {
                    var nameClaim = token.Claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == "sub" || c.Type == "name");
                    if (nameClaim != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Name, nameClaim.Value));
                    }
                }

                // Ensure we have a role claim
                if (!claims.Any(c => c.Type == ClaimTypes.Role))
                {
                    var roleClaim = token.Claims.FirstOrDefault(c => c.Type == "role");
                    if (roleClaim != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));
                    }
                }
            }
            catch
            {
                // If token parsing fails, return empty claims
                return claims;
            }

            return claims;
        }
    }
}
