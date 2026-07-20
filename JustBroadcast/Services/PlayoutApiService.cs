using System.Text.Json;
using Blazored.LocalStorage;
using JustBroadcast.Models;
using Microsoft.Extensions.Configuration;

namespace JustBroadcast.Services
{
    // Wraps the six single-playout REST endpoints, reusing the app's existing
    // request pattern (ngrok-skip header + Bearer token + BaseUrl from config).
    public class PlayoutApiService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILocalStorageService localStorage) : IPlayoutApiService
    {
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        private string BaseUrl => configuration["ApiSettings:BaseUrl"] ?? "http://178.222.112.105:5016";
        private bool UseMock => Convert.ToBoolean(configuration["ApiSettings:UseMockPlayoutData"]);

        public Task<List<PlayoutListInfo>> GetPlayoutsShortAsync() =>
            GetListAsync<PlayoutListInfo>(configuration["ApiSettings:PlayoutsEndpoint"] ?? "/api/Playouts/short");

        public Task<List<ErrorListDto>> GetErrorsLastWeekAsync(string playoutId) =>
            UseMock ? Task.FromResult(MockPlayoutData.Errors(playoutId))
                    : GetListAsync<ErrorListDto>($"/api/Errors/lastweek/playout/{playoutId}");

        public Task<List<PlayoutoutputShortDto>> GetOutputsAsync(string playoutId) =>
            UseMock ? Task.FromResult(MockPlayoutData.Outputs(playoutId))
                    : GetListAsync<PlayoutoutputShortDto>($"/api/Playoutoutputs/playout/{playoutId}");

        public Task<NextScheduleItemDto?> GetNextPushedListAsync(string channelId) =>
            UseMock ? Task.FromResult<NextScheduleItemDto?>(MockPlayoutData.PushedList())
                    : GetSingleAsync<NextScheduleItemDto>($"/api/Schedulelists/pushed/getnext/channel/{channelId}");

        public Task<NextScheduleItemDto?> GetNextDailyListAsync(string channelId) =>
            UseMock ? Task.FromResult<NextScheduleItemDto?>(MockPlayoutData.DailyList())
                    : GetSingleAsync<NextScheduleItemDto>($"/api/Schedulelists/daily/getnext/channel/{channelId}");

        public Task<NextScheduleItemDto?> GetNextScheduleItemAsync(string channelId) =>
            UseMock ? Task.FromResult<NextScheduleItemDto?>(MockPlayoutData.NextItem())
                    : GetSingleAsync<NextScheduleItemDto>($"/api/Scheduleitems/channel/next/{channelId}");

        public Task<List<EventlogDto>> GetEventLogsAsync(string playoutId) =>
            UseMock ? Task.FromResult(MockPlayoutData.EventLogs(playoutId))
                    : GetListAsync<EventlogDto>($"/api/Eventlogs/20/playout/{playoutId}");

        private async Task<List<T>> GetListAsync<T>(string path)
        {
            var json = await GetRawAsync(path);
            if (string.IsNullOrEmpty(json)) return new List<T>();
            try
            {
                return JsonSerializer.Deserialize<List<T>>(json, JsonOpts) ?? new List<T>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PlayoutApiService] Deserialize list failed for {path}: {ex.Message}");
                return new List<T>();
            }
        }

        private async Task<T?> GetSingleAsync<T>(string path) where T : class
        {
            var json = await GetRawAsync(path);
            if (string.IsNullOrEmpty(json) || json.Trim() == "null") return null;
            try
            {
                return JsonSerializer.Deserialize<T>(json, JsonOpts);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PlayoutApiService] Deserialize failed for {path}: {ex.Message}");
                return null;
            }
        }

        private async Task<string?> GetRawAsync(string path)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}{path}");
                request.Headers.Add("ngrok-skip-browser-warning", "true");

                var accessToken = await localStorage.GetItemAsync<string>("authToken");
                if (!string.IsNullOrEmpty(accessToken))
                    request.Headers.Add("Authorization", $"Bearer {accessToken}");

                var response = await httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[PlayoutApiService] {path} -> {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PlayoutApiService] {path} exception: {ex.Message}");
                return null;
            }
        }
    }
}
