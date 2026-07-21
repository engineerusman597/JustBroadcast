using System.Text.Json;
using JustBroadcast.Models;
using JustBroadcast.Services;
using Microsoft.AspNetCore.Components;

namespace JustBroadcast.Pages
{
    public partial class AllPlayouts : IDisposable
    {
        [Inject] private IMockPlayoutFeed MockFeed { get; set; } = default!;
        [Inject] private IPlayoutApiService PlayoutApi { get; set; } = default!;
        [Inject] private IPlayoutSettingsService Settings { get; set; } = default!;

        private bool isLoading = true;
        private List<PlayoutListDto> playouts = new();
        private readonly Dictionary<string, PlaylistInfoMessageDto> _live = new();

        // Per-playout data that SignalR doesn't carry. Output *names* and types come
        // from REST once on load; live FPS/health then arrive inside PlayoutStatus.
        private readonly Dictionary<string, List<PlayoutoutputShortDto>> _outputs = new();
        private readonly Dictionary<string, List<ErrorListDto>> _errors = new();
        private readonly Dictionary<string, List<EventlogDto>> _eventLogs = new();
        private readonly Dictionary<string, int[]> _metricSlots = new();

        // SignalR is the only source of live online state; Playouts/short returns
        // stale DB columns, not current status.
        private readonly HashSet<string> _online = new();

        protected override void OnInitialized()
        {
            SignalRService.CommandReceived += OnCommandReceived;
            MockFeed.Tick += OnMockTick;
        }

        protected override async Task OnInitializedAsync()
        {
            var user = await AuthService.GetCurrentUserAsync();
            playouts = user?.AllPlayouts ?? new List<PlayoutListDto>();

            // 1 playout -> skip the grid, go straight to its single view.
            if (playouts.Count == 1)
            {
                Navigation.NavigateTo($"playout/{playouts[0].Id}");
                return;
            }

            if (MockFeed.Enabled)
            {
                MockFeed.Register(playouts.Select(p => p.Id));
                MockFeed.Start();
            }

            await LoadPerPlayoutData();

            isLoading = false;
        }

        // One-time load of everything the tiles need that SignalR doesn't provide.
        // Only fetched when the current density actually renders it.
        private async Task LoadPerPlayoutData()
        {
            var density = GridColumns;

            foreach (var p in playouts)
            {
                var slots = new int[5];
                for (int slot = 0; slot < slots.Length; slot++)
                    slots[slot] = await Settings.GetMetricTypeAsync(p.Id, slot);
                _metricSlots[p.Id] = slots;

                try
                {
                    // Output names/types are needed at every density.
                    _outputs[p.Id] = await PlayoutApi.GetOutputsAsync(p.Id);

                    // Errors and event logs only appear in the 2-up tiles.
                    if (density == 2)
                    {
                        _errors[p.Id] = await PlayoutApi.GetErrorsLastWeekAsync(p.Id);
                        _eventLogs[p.Id] = await PlayoutApi.GetEventLogsAsync(p.Id);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AllPlayouts] load for '{p.Id}' failed: {ex.Message}");
                }
            }
        }

        private void OnMockTick(string id, PlaylistInfoMessageDto dto)
        {
            InvokeAsync(() =>
            {
                _live[id] = dto;
                _online.Add(id);
                StateHasChanged();
            });
        }

        // 2->2-up, 3-4->4-up, 5-6->6-up, 7-8->8-up (cap at 8).
        private int GridColumns
        {
            get
            {
                var n = playouts.Count;
                if (n <= 2) return 2;
                if (n <= 4) return 4;
                if (n <= 6) return 6;
                return 8;
            }
        }

        private string ViewLabel => $"{GridColumns}-up view";

        private bool IsOnline(string id) => _online.Contains(id);


        private PlaylistInfoMessageDto? LiveFor(string id) =>
            _live.TryGetValue(id, out var d) ? d : null;

        private List<PlayoutoutputShortDto>? OutputsFor(string id) =>
            _outputs.TryGetValue(id, out var o) ? o : null;

        private List<ErrorListDto>? ErrorsFor(string id) =>
            _errors.TryGetValue(id, out var e) ? e : null;

        private List<EventlogDto>? EventLogsFor(string id) =>
            _eventLogs.TryGetValue(id, out var l) ? l : null;

        private int[]? MetricSlotsFor(string id) =>
            _metricSlots.TryGetValue(id, out var m) ? m : null;

        private async Task OnMetricTypeChanged(string playoutId, int slot, int metricType)
        {
            if (_metricSlots.TryGetValue(playoutId, out var slots) && slot < slots.Length)
                slots[slot] = metricType;

            await Settings.SetMetricTypeAsync(playoutId, slot, metricType);
            StateHasChanged();
        }

        private async Task StartStopOutput(StartStopOuputDto dto) =>
            await SignalRService.SendStartStopOutputAsync(dto);

        private void OnCommandReceived(CommandDto command)
        {
            if (command.command == ServiceMessages.PlayoutStatus.ToString())
            {
                if (command.data == null) return;
                try
                {
                    var json = command.data is string s ? s : JsonSerializer.Serialize(command.data);
                    var dto = JsonSerializer.Deserialize<PlaylistInfoMessageDto>(
                        json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (dto != null)
                    {
                        // Prefer the id carried in the payload; fall back to the envelope's clientId.
                        var id = !string.IsNullOrEmpty(dto.PlayoutId) ? dto.PlayoutId! : command.clientId;
                        Console.WriteLine($"[AllPlayouts] PlayoutStatus recv - clientId='{command.clientId}' dto.PlayoutId='{dto.PlayoutId}' -> key '{id}' | known tiles: {string.Join(",", playouts.Select(p => p.Id))}");
                        InvokeAsync(() =>
                        {
                            _live[id] = dto;
                            _online.Add(id);
                            StateHasChanged();
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[AllPlayouts] PlayoutStatus parse failed: {ex.Message}");
                }
            }
            else if (command.command == ServiceMessages.ClientStatusChanged.ToString())
            {
                // Only playout servers drive tile online state (other groups are users).
                if (command.group != ClientType.PlayoutServer.ToString()) return;

                var id = command.clientId;
                var online = command.data?.ToString() == "1";
                Console.WriteLine($"[AllPlayouts] ClientStatusChanged - playout '{id}' online={online}");
                InvokeAsync(() =>
                {
                    if (online) _online.Add(id);
                    else { _online.Remove(id); _live.Remove(id); }
                    StateHasChanged();
                });
            }
        }

        private void SelectPlayout(string playoutId) =>
            Navigation.NavigateTo($"playout/{playoutId}");

        public void Dispose()
        {
            SignalRService.CommandReceived -= OnCommandReceived;
            MockFeed.Tick -= OnMockTick;
        }
    }
}
