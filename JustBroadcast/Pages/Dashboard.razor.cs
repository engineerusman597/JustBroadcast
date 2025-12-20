using JustBroadcast.Models;
using System.Net.Http.Json;

namespace JustBroadcast.Pages
{
    public partial class Dashboard
    {
        private DashboardData? dashboardData;
        private bool isLoading = true;
        private List<ErrorFrequencyItem> errorFrequencyData = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadDashboardData();
        }

        private async Task LoadDashboardData()
        {
            isLoading = true;

            try
            {
                // Get API endpoint from configuration
                var apiUrl = Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7000";
                var playoutsEndpoint = Configuration["ApiSettings:PlayoutsEndpoint"] ?? "/api/Playouts/short";

                // Initialize with mock data structure
                LoadMockData();

                // Try to fetch Active Playouts from API
                try
                {
                    // Create request with ngrok bypass header
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{apiUrl}{playoutsEndpoint}");
                    request.Headers.Add("ngrok-skip-browser-warning", "true");

                    var response = await HttpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        // Read the raw JSON response first
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"API Response JSON: {jsonResponse}");

                        // Deserialize from the string instead of the stream
                        var playouts = System.Text.Json.JsonSerializer.Deserialize<List<PlayoutListDto>>(jsonResponse, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (playouts != null && dashboardData != null)
                        {
                            // Map API response to ActivePlayout model
                            dashboardData.ActivePlayouts = [.. playouts.Select(p => new ActivePlayout
                            {
                                PlayoutName = p.Name ?? "Unknown",
                                ChannelName = p.Channel ?? "Unassigned",
                                Status = "online", // Default status - will be updated when API provides this
                                Playing = "playing", // Default playing state - will be updated when API provides this
                                Spare = p.Spare ?? false
                            })];

                            // Update playout stats
                            dashboardData.PlayoutStats.Total = playouts.Count;
                            dashboardData.PlayoutStats.Running = playouts.Count;
                            dashboardData.PlayoutStats.Offline = 0;
                        }
                    }
                    else
                    {
                        // Log the error response
                        Console.WriteLine($"API Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception details
                    Console.WriteLine($"Exception loading playouts: {ex.GetType().Name}");
                    Console.WriteLine($"Message: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    // Keep mock data for Active Playouts if API call fails
                }
            }
            catch(Exception ex)
            {
                // Use mock data if exception occurs
                LoadMockData();
            }

            // Prepare error frequency data for chart
            if (dashboardData != null)
            {
                errorFrequencyData =
            [
                new() { Day = "Mon", Count = dashboardData.ErrorFrequency.GetValueOrDefault("Mon", 0) },
                new() { Day = "Tue", Count = dashboardData.ErrorFrequency.GetValueOrDefault("Tue", 0) },
                new() { Day = "Wed", Count = dashboardData.ErrorFrequency.GetValueOrDefault("Wed", 0) },
                new() { Day = "Thu", Count = dashboardData.ErrorFrequency.GetValueOrDefault("Thu", 0) },
                new() { Day = "Fri", Count = dashboardData.ErrorFrequency.GetValueOrDefault("Fri", 0) },
                new() { Day = "Sat", Count = dashboardData.ErrorFrequency.GetValueOrDefault("Sat", 0) },
                new() { Day = "Sun", Count = dashboardData.ErrorFrequency.GetValueOrDefault("Sun", 0) }
            ];
            }

            isLoading = false;
        }

        private void LoadMockData()
        {
            dashboardData = new DashboardData
            {
                PlayoutStats = new PlayoutStats { Total = 4, Running = 2, Offline = 2 },
                ChannelStats = new ChannelStats { Total = 3, Assigned = 3 },
                UserStats = new UserStats { Total = 1, Online = 1 },
                MediaAssetStats = new MediaAssetStats { Total = 1247, AddedThisWeek = 24 },
                AlertStats = new AlertStats { Total = 3, RequireAttention = 2 },
                ActivePlayouts =
            [
                new() { PlayoutName = "Main Channel HD", ChannelName = "Channel 1", Status = "online", Playing = "playing", Spare = false },
                new() { PlayoutName = "Sports Channel", ChannelName = "Channel 2", Status = "online", Playing = "playing", Spare = false },
                new() { PlayoutName = "Entertainment", ChannelName = "Channel 3", Status = "online", Playing = "playing", Spare = false },
                new() { PlayoutName = "News 24/7", ChannelName = "Channel 4", Status = "online", Playing = "scheduled", Spare = true },
                new() { PlayoutName = "Music Channel", ChannelName = "Channel 5", Status = "online", Playing = "playing", Spare = false },
                new() { PlayoutName = "Documentary", ChannelName = "Channel 6", Status = "offline", Playing = "error", Spare = true },
                new() { PlayoutName = "Kids Channel", ChannelName = "Channel 7", Status = "online", Playing = "playing", Spare = false },
                new() { PlayoutName = "Movies HD", ChannelName = "Channel 8", Status = "online", Playing = "scheduled", Spare = false }
            ],
                SystemResources = new SystemResources
                {
                    CpuUsage = 58,
                    GpuUsage = 45,
                    RamUsage = 68,
                    CpuHistory = GenerateCpuHistory()
                },
                ErrorFeed =
            [
                new() { Message = "Network timeout", TimeAgo = "0 min ago ago", Type = "error" },
                new() { Message = "Audio format mismatch on Playout Entertainment", TimeAgo = "0 min ago ago", Type = "warning" },
                new() { Message = "Buffer underrun", TimeAgo = "15 min ago", Type = "warning" },
                new() { Message = "Video engine cash", TimeAgo = "1 h ago ago", Type = "error" },
                new() { Message = "Stream connection lost", TimeAgo = "1.5 h ago", Type = "warning" },
                new() { Message = "Network timeout", TimeAgo = "0 min ago ago", Type = "error" }
            ],
                Alerts =
            [
                new() { Message = "Output frames drop", TimeAgo = "3 ms ago ago", Type = "info" },
                new() { Message = "Ad breaks missed", TimeAgo = "35 m ago ago", Type = "warning" },
                new() { Message = "Audio channels mis", TimeAgo = "11 ago ago", Type = "warning" },
                new() { Message = "FPS drops", TimeAgo = "15 ago ago", Type = "warning" }
            ],
                ErrorFrequency = new Dictionary<string, int>
            {
                { "Mon", 75 },
                { "Tue", 60 },
                { "Wed", 85 },
                { "Thu", 55 },
                { "Fri", 45 },
                { "Sat", 50 },
                { "Sun", 95 }
            }
            };
        }

        private List<CpuDataPoint> GenerateCpuHistory()
        {
            var history = new List<CpuDataPoint>();
            var random = new Random();
            var now = DateTime.Now;

            for (int i = 20; i >= 0; i--)
            {
                history.Add(new CpuDataPoint
                {
                    Time = now.AddMinutes(-i),
                    Value = random.Next(40, 70)
                });
            }

            return history;
        }

        private string GetAlertIcon(string type) => type switch
        {
            "error" => "remove",
            "warning" => "warning",
            "info" => "info",
            _ => "info"
        };

        public class ErrorFrequencyItem
        {
            public string Day { get; set; } = string.Empty;
            public int Count { get; set; }
        }
    }
}