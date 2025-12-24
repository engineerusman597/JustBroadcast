using Blazored.LocalStorage;
using JustBroadcast.Models;
using JustBroadcast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace JustBroadcast.Pages
{
    public partial class Dashboard : IDisposable
    {
        private DashboardData? dashboardData;
        private bool isLoading = true;
        private List<ErrorFrequencyItem> errorFrequencyData = new();
        private string _clientId = Guid.NewGuid().ToString();
        private bool telemetryEnabled = true;
        private string lastMetricsUpdate = "Never";

        protected override async Task OnInitializedAsync()
        {
            // Subscribe to SignalR events
            SignalRService.CommandReceived += OnCommandReceived;
            SignalRService.ConnectionStateChanged += OnConnectionStateChanged;

            await LoadDashboardData();
            await InitializeSignalR();
        }

        private async Task InitializeSignalR()
        {
            Console.WriteLine($"[SignalR] InitializeSignalR started");
            try
            {
                var accessToken = await LocalStorage.GetItemAsync<string>("authToken");
                Console.WriteLine($"[SignalR] Access token retrieved: {!string.IsNullOrEmpty(accessToken)}");
                Console.WriteLine($"[SignalR] Access token length: {accessToken?.Length ?? 0}");

                if (!string.IsNullOrEmpty(accessToken))
                {
                    Console.WriteLine($"[SignalR] Starting SignalR connection...");
                    await SignalRService.StartAsync(accessToken);

                    // Wait a moment for connection to establish
                    await Task.Delay(500);

                    // Request status sync from all playout apps
                    if (SignalRService.IsConnected)
                    {
                        Console.WriteLine($"[SignalR] Connected! Sending RequestStatusSync with clientId: {_clientId}");
                        await SignalRService.SendRequestStatusSync(_clientId);
                    }
                    else
                    {
                        Console.WriteLine($"[SignalR] Warning: SignalR not connected after StartAsync");
                    }
                }
                else
                {
                    Console.WriteLine($"[SignalR] Error: Access token is empty, cannot initialize SignalR");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SignalR] ERROR initializing SignalR");
                Console.WriteLine($"[SignalR] Exception: {ex.Message}");
                Console.WriteLine($"[SignalR] Stack trace: {ex.StackTrace}");
            }
        }

        private void OnConnectionStateChanged(string state)
        {
            Console.WriteLine($"[SignalR] Connection state changed: {state}");

            if (dashboardData?.ActivePlayouts != null)
            {
                switch (state)
                {
                    case "Connected":
                        Console.WriteLine($"[SignalR] Connected - Setting all playouts to OFFLINE");
                        // Set all playouts to OFFLINE when connected
                        foreach (var playout in dashboardData.ActivePlayouts)
                        {
                            playout.IsOnline = 0;
                            playout.Status = "offline";
                        }
                        break;

                    case "Closed":
                        Console.WriteLine($"[SignalR] Closed - Setting all playouts to UNKNOWN");
                        // Set all playouts to UNKNOWN when disconnected
                        foreach (var playout in dashboardData.ActivePlayouts)
                        {
                            playout.IsOnline = 2;
                            playout.Status = "unknown";
                        }
                        break;
                }

                InvokeAsync(StateHasChanged);
            }
            else
            {
                Console.WriteLine($"[SignalR] Warning: dashboardData or ActivePlayouts is null");
            }
        }

        private void OnCommandReceived(CommandDto command)
        {
            Console.WriteLine($"[SignalR] Command received: {command.command}");
            Console.WriteLine($"[SignalR] ClientId: {command.clientId}, Group: {command.group}");

            if (command.command == ServiceMessages.ClientStatusChanged.ToString())
            {
                Console.WriteLine($"[SignalR] Processing ClientStatusChanged command");
                if (!string.IsNullOrEmpty(command.group) && command.group == ClientType.PlayoutServer.ToString())
                {
                    PlayoutStatusChanged(command);
                }
            }
            else if (command.command == ServiceMessages.Metrics.ToString())
            {
                Console.WriteLine($"[SignalR] Processing Metrics command");
                SystemMetricsReceived(command);
            }
            else
            {
                Console.WriteLine($"[SignalR] Unknown command type: {command.command}");
            }
        }

        private void PlayoutStatusChanged(CommandDto command)
        {
            if (dashboardData?.ActivePlayouts == null) return;

            // Find the playout by clientId and update its status
            var playout = dashboardData.ActivePlayouts.FirstOrDefault(p => p.Id == command.clientId);
            if (playout != null)
            {
                // Update status from command data
                // For now, set to ON AIR when we receive status
                playout.IsOnline = 1;
                playout.Status = "online";

                // Also update playing status (when online, it's playing; when offline, it's stopped)
                playout.IsPlaying = 1;
                playout.Playing = "playing";

                UpdatePlayoutStats();
                InvokeAsync(StateHasChanged);
            }
        }

        private void SystemMetricsReceived(CommandDto command)
        {
            Console.WriteLine($"[Metrics] SystemMetricsReceived called");

            if (dashboardData?.SystemResources == null)
            {
                Console.WriteLine($"[Metrics] ERROR: dashboardData or SystemResources is null");
                return;
            }

            try
            {
                Console.WriteLine($"[Metrics] Raw command.data type: {command.data?.GetType().Name ?? "null"}");

                // Parse the metrics data from command.data
                var json = System.Text.Json.JsonSerializer.Serialize(command.data);
                Console.WriteLine($"[Metrics] Serialized JSON: {json}");

                var metrics = System.Text.Json.JsonSerializer.Deserialize<SystemMetricsDto>(json);

                if (metrics != null)
                {
                    Console.WriteLine($"[Metrics] Parsed metrics - CPU: {metrics.cpuPercent}%, GPU: {metrics.gpuPercent}%, RAM: {metrics.ramPercent}%");

                    // Update system resources
                    dashboardData.SystemResources.CpuUsage = metrics.cpuPercent;
                    dashboardData.SystemResources.GpuUsage = metrics.gpuPercent;
                    dashboardData.SystemResources.RamUsage = metrics.ramPercent;

                    Console.WriteLine($"[Metrics] Updated SystemResources - CPU: {dashboardData.SystemResources.CpuUsage}%, GPU: {dashboardData.SystemResources.GpuUsage}%, RAM: {dashboardData.SystemResources.RamUsage}%");

                    // Create new list to force chart rebind
                    var newHistory = new List<CpuDataPoint>(dashboardData.SystemResources.CpuHistory);

                    // Add new CPU data point
                    newHistory.Add(new CpuDataPoint
                    {
                        Time = DateTime.Now,
                        Value = metrics.cpuPercent
                    });

                    // Keep only last 30 data points (rolling buffer)
                    if (newHistory.Count > 30)
                    {
                        newHistory.RemoveAt(0);
                        Console.WriteLine($"[Metrics] Removed oldest CPU history point");
                    }

                    // Replace the list to trigger chart update
                    dashboardData.SystemResources.CpuHistory = newHistory;

                    Console.WriteLine($"[Metrics] CPU History count: {dashboardData.SystemResources.CpuHistory.Count}");
                    Console.WriteLine($"[Metrics] Invoking StateHasChanged to refresh UI");
                    lastMetricsUpdate = DateTime.Now.ToString("HH:mm:ss");
                    InvokeAsync(StateHasChanged);
                }
                else
                {
                    Console.WriteLine($"[Metrics] ERROR: Failed to deserialize metrics (result was null)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Metrics] ERROR: Exception processing system metrics");
                Console.WriteLine($"[Metrics] Exception type: {ex.GetType().Name}");
                Console.WriteLine($"[Metrics] Exception message: {ex.Message}");
                Console.WriteLine($"[Metrics] Stack trace: {ex.StackTrace}");
            }
        }

        private async Task ToggleTelemetry()
        {
            // Toggle the state
            telemetryEnabled = !telemetryEnabled;

            try
            {
                var apiUrl = Configuration["ApiSettings:BaseUrl"] ?? "http://178.222.112.105:5016";
                var telemetryEndpoint = Configuration["ApiSettings:SystemSettingsTelemetryEndpoint"] ?? "/api/Systemsettings/settelemetrycontrol";

                var request = new HttpRequestMessage(HttpMethod.Post, $"{apiUrl}{telemetryEndpoint}?enabled={telemetryEnabled}");
                request.Headers.Add("ngrok-skip-browser-warning", "true");

                var accessToken = await LocalStorage.GetItemAsync<string>("accessToken");
                if (!string.IsNullOrEmpty(accessToken))
                {
                    request.Headers.Add("Authorization", $"Bearer {accessToken}");
                }

                var response = await HttpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Telemetry {(telemetryEnabled ? "enabled" : "disabled")} successfully");
                }
                else
                {
                    Console.WriteLine($"Telemetry API Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception toggling telemetry: {ex.Message}");
            }
        }

        private async Task LoadDashboardData()
        {
            isLoading = true;

            try
            {
                // Get API endpoint from configuration
                var apiUrl = Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7000";
                var playoutsEndpoint = Configuration["ApiSettings:PlayoutsEndpoint"] ?? "/api/Playouts/short";
                var channelsEndpoint = Configuration["ApiSettings:ChannelsCountEndpoint"] ?? "/api/Channels/count";

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
                        var playouts = System.Text.Json.JsonSerializer.Deserialize<List<PlayoutListInfo>>(
                            jsonResponse,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (playouts != null && dashboardData != null)
                        {
                            // Map API response to ActivePlayout model
                            dashboardData.ActivePlayouts = playouts.Select(p => new ActivePlayout
                            {
                                Id = p.Id,
                                PlayoutName = p.Name ?? "Unknown",
                                ChannelName = p.Channel ?? "Unassigned",
                                Spare = p.Spare ?? false,
                                IsOnline = p.IsOnline,
                                IsPlaying = p.IsPlaying,
                                Status = GetStatusText(p.IsOnline),
                                Playing = GetPlayingText(p.IsPlaying)
                            }).ToList();

                            UpdatePlayoutStats();
                        }
                    }
                    else
                    {
                        Console.WriteLine($"API Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception loading playouts: {ex.GetType().Name}");
                    Console.WriteLine($"Message: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }

                // Try to fetch Channels count from API
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{apiUrl}{channelsEndpoint}");
                    request.Headers.Add("ngrok-skip-browser-warning", "true");

                    var response = await HttpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Channels API Response: {jsonResponse}");

                        var channelsData = System.Text.Json.JsonSerializer.Deserialize<ChannelListCountDto>(
                            jsonResponse,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (channelsData != null && dashboardData != null)
                        {
                            dashboardData.ChannelStats.Total = channelsData.TotalChannels ?? 0;
                            dashboardData.ChannelStats.Assigned = channelsData.Connected ?? 0;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Channels API Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception loading channels: {ex.GetType().Name}");
                    Console.WriteLine($"Message: {ex.Message}");
                }

                // Try to fetch Errors from last week
                var errorsEndpoint = Configuration["ApiSettings:ErrorsLastWeekEndpoint"] ?? "/api/Errors/lastweek";
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{apiUrl}{errorsEndpoint}");
                    request.Headers.Add("ngrok-skip-browser-warning", "true");

                    var response = await HttpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Errors API Response: {jsonResponse}");

                        var errorsData = System.Text.Json.JsonSerializer.Deserialize<List<ErrorListDto>>(
                            jsonResponse,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (errorsData != null && dashboardData != null)
                        {
                            UpdateErrorsData(errorsData);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Errors API Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception loading errors: {ex.GetType().Name}");
                    Console.WriteLine($"Message: {ex.Message}");
                }

                // Try to fetch Assets count
                var assetsEndpoint = Configuration["ApiSettings:AssetsCountEndpoint"] ?? "/api/Assets/count";
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, $"{apiUrl}{assetsEndpoint}");
                    request.Headers.Add("ngrok-skip-browser-warning", "true");

                    var response = await HttpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Assets API Response: {jsonResponse}");

                        var assetsData = System.Text.Json.JsonSerializer.Deserialize<SystemMetricsDto>(
                            jsonResponse,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (assetsData != null && dashboardData != null)
                        {
                            dashboardData.MediaAssetStats.Total = assetsData.cpuPercent;
                            dashboardData.MediaAssetStats.AddedThisWeek = assetsData.gpuPercent;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Assets API Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception loading assets: {ex.GetType().Name}");
                    Console.WriteLine($"Message: {ex.Message}");
                }
            }
            catch(Exception ex)
            {
                LoadMockData();
            }

            // Prepare error frequency data for chart
            if (dashboardData != null)
            {
                errorFrequencyData = new List<ErrorFrequencyItem>
                {
                    new() { Day = "Mon", Count = dashboardData.ErrorFrequency.GetValueOrDefault("Mon", 0) },
                    new() { Day = "Tue", Count = dashboardData.ErrorFrequency.GetValueOrDefault("Tue", 0) },
                    new() { Day = "Wed", Count = dashboardData.ErrorFrequency.GetValueOrDefault("Wed", 0) },
                    new() { Day = "Thu", Count = dashboardData.ErrorFrequency.GetValueOrDefault("Thu", 0) },
                    new() { Day = "Fri", Count = dashboardData.ErrorFrequency.GetValueOrDefault("Fri", 0) },
                    new() { Day = "Sat", Count = dashboardData.ErrorFrequency.GetValueOrDefault("Sat", 0) },
                    new() { Day = "Sun", Count = dashboardData.ErrorFrequency.GetValueOrDefault("Sun", 0) }
                };
            }

            isLoading = false;
        }

        private string GetStatusText(int isOnline)
        {
            return isOnline switch
            {
                0 => "offline",
                1 => "online",
                2 => "unknown",
                _ => "offline"
            };
        }

        private string GetPlayingText(int isPlaying)
        {
            return isPlaying switch
            {
                0 => "stopped",
                1 => "playing",
                _ => "stopped"
            };
        }

        private void UpdatePlayoutStats()
        {
            if (dashboardData?.ActivePlayouts == null) return;

            var nonSpare = dashboardData.ActivePlayouts.Where(p => !p.Spare).ToList();
            var spare = dashboardData.ActivePlayouts.Where(p => p.Spare).Count();

            dashboardData.PlayoutStats.Total = dashboardData.ActivePlayouts.Count;
            dashboardData.PlayoutStats.Running = nonSpare.Count(p => p.IsOnline == 1);
            dashboardData.PlayoutStats.Offline = nonSpare.Count(p => p.IsOnline == 0);
        }

        private string GetPlayoutStatusText()
        {
            if (dashboardData?.ActivePlayouts == null) return "";

            var nonSpare = dashboardData.ActivePlayouts.Where(p => !p.Spare).ToList();
            var spare = dashboardData.ActivePlayouts.Where(p => p.Spare).Count();

            var running = nonSpare.Count(p => p.IsOnline == 1);
            var playing = nonSpare.Count(p => p.IsPlaying == 1);

            return $"{running} running, {playing} playing" + (spare > 0 ? $", {spare} spare" : "");
        }

        private string GetChannelStatusText()
        {
            if (dashboardData == null) return "";

            var unassigned = dashboardData.ChannelStats.Total - dashboardData.ChannelStats.Assigned;

            if (unassigned == 0)
            {
                return "All channels assigned";
            }
            else
            {
                return $"{unassigned} channel{(unassigned > 1 ? "s" : "")} not assigned";
            }
        }

        private string GetAlertStatusText()
        {
            if (dashboardData == null) return "";

            if (dashboardData.AlertStats.RequireAttention == 0)
            {
                return "No critical errors";
            }
            else
            {
                return $"{dashboardData.AlertStats.RequireAttention} require attention";
            }
        }

        private string GetMediaAssetChangeText()
        {
            if (dashboardData == null) return "";

            var change = dashboardData.MediaAssetStats.AddedThisWeek;

            if (change > 0)
            {
                return $"+{change} this week";
            }
            else if (change < 0)
            {
                return $"{change} this week";
            }
            else
            {
                return "No change this week";
            }
        }

        private void UpdateErrorsData(List<ErrorListDto> errors)
        {
            if (dashboardData == null) return;

            // Update Alert Stats
            dashboardData.AlertStats.Total = errors.Count;
            dashboardData.AlertStats.RequireAttention = errors.Count(e => e.Type == 0); // Type 0 = error

            // Update Alerts list (now Errors section in UI)
            dashboardData.Alerts = errors.Select(e => new AlertItem
            {
                Message = e.Description,
                TimeAgo = $"{e.Time:dd.MM.yyyy} @ {e.Time:HH:mm:ss}",
                Type = e.Type == 0 ? "error" : e.Type == 1 ? "warning" : "info",
                PlayoutName = e.PlayoutName
            }).ToList();

            // Update Error Frequency for chart (group by day of week for last 7 days)
            var now = DateTime.Now;
            var last7Days = Enumerable.Range(0, 7).Select(i => now.AddDays(-i).Date).ToList();

            dashboardData.ErrorFrequency = new Dictionary<string, int>();

            foreach (var day in last7Days)
            {
                var dayName = day.ToString("ddd");
                var count = errors.Count(e => e.Time.Date == day);
                dashboardData.ErrorFrequency[dayName] = count;
            }
        }

        private void LoadMockData()
        {
            dashboardData = new DashboardData
            {
                PlayoutStats = new PlayoutStats { Total = 7, Running = 1, Offline = 5 },
                ChannelStats = new ChannelStats { Total = 3, Assigned = 3 },
                UserStats = new UserStats { Total = 1, Online = 1 },
                MediaAssetStats = new MediaAssetStats { Total = 1247, AddedThisWeek = 24 },
                AlertStats = new AlertStats { Total = 3, RequireAttention = 2 },
                ActivePlayouts = new List<ActivePlayout>
                {
                    new() { Id = "1", PlayoutName = "I9 PC 2", ChannelName = "", Status = "offline", Playing = "stopped", Spare = false, IsOnline = 0, IsPlaying = 0 },
                    new() { Id = "2", PlayoutName = "I9 ULTIMATE", ChannelName = "JB1", Status = "online", Playing = "stopped", Spare = false, IsOnline = 1, IsPlaying = 0 },
                    new() { Id = "3", PlayoutName = "I9 ULTIMATE 2", ChannelName = "JB2", Status = "offline", Playing = "stopped", Spare = false, IsOnline = 0, IsPlaying = 0 },
                    new() { Id = "4", PlayoutName = "I9 ULTIMATE 3", ChannelName = "JB3", Status = "offline", Playing = "stopped", Spare = false, IsOnline = 0, IsPlaying = 0 },
                    new() { Id = "5", PlayoutName = "I9 ULTIMATE 4", ChannelName = "JB4", Status = "offline", Playing = "stopped", Spare = false, IsOnline = 0, IsPlaying = 0 },
                    new() { Id = "6", PlayoutName = "SPARE DRUGI PC", ChannelName = "", Status = "offline", Playing = "stopped", Spare = true, IsOnline = 0, IsPlaying = 0 },
                    new() { Id = "7", PlayoutName = "SPARE I9", ChannelName = "", Status = "offline", Playing = "stopped", Spare = true, IsOnline = 0, IsPlaying = 0 }
                },
                SystemResources = new SystemResources
                {
                    CpuUsage = 58,
                    GpuUsage = 45,
                    RamUsage = 68,
                    CpuHistory = GenerateCpuHistory()
                },
                ErrorFeed = new List<ErrorItem>
                {
                    new() { Message = "Stream connection lost", TimeAgo = "17.12.2025 | live.m3u8 @ 2:59:59", Type = "error" },
                    new() { Message = "Network timeout", TimeAgo = "16.12.2025 | live.m3u8 @ 2:31:31", Type = "warning" },
                    new() { Message = "Buffer underrun", TimeAgo = "16.12.2025 | live.m3u8 23:02:31", Type = "warning" }
                },
                ActiveUsers = new List<ActiveUser>
                {
                    new() { Name = "John Doe", Avatar = "", Role = "Remote Control", LastSeen = "last seen less than 1 min ago", RoleBadge = "JB1" }
                },
                Alerts = new List<AlertItem>
                {
                    new() { Message = "Output frames drop", TimeAgo = "3 ms ago ago", Type = "info" },
                    new() { Message = "Ad breaks missed", TimeAgo = "35 m ago ago", Type = "warning" },
                    new() { Message = "Audio channels mis", TimeAgo = "11 ago ago", Type = "warning" },
                    new() { Message = "FPS drops", TimeAgo = "15 ago ago", Type = "warning" }
                },
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

        private string GetUserInitials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "U";

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[1][0]}".ToUpper();

            return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
        }

        public void Dispose()
        {
            SignalRService.CommandReceived -= OnCommandReceived;
            SignalRService.ConnectionStateChanged -= OnConnectionStateChanged;
        }

        public class ErrorFrequencyItem
        {
            public string Day { get; set; } = string.Empty;
            public int Count { get; set; }
        }
    }
}
