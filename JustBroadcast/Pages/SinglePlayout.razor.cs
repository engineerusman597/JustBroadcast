using System.Text.Json;
using JustBroadcast.Models;
using JustBroadcast.Services;
using JustBroadcast.Shared.Playout;
using Microsoft.AspNetCore.Components;

namespace JustBroadcast.Pages
{
    public partial class SinglePlayout : IDisposable
    {
        [Parameter] public string Id { get; set; } = string.Empty;

        [Inject] private IAuthService AuthService { get; set; } = default!;
        [Inject] private IMockPlayoutFeed MockFeed { get; set; } = default!;

        private bool isLoading = true;
        private string PlayoutName = "Playout";
        private string ChannelId = string.Empty;
        private bool IsOnline;

        private PlaylistInfoMessageDto? live;
        private List<PlayoutoutputShortDto> outputs = new();
        private List<ErrorListDto> errors = new();
        private List<EventlogDto> eventLogs = new();
        private NextScheduleItemDto? pushedList;
        private NextScheduleItemDto? dailyList;
        private NextScheduleItemDto? nextItem;

        private bool scte35Selected = true;
        private readonly int[] graphTypes = { 0, 1, 2, 3, 4 };
        private readonly MetricGraph?[] graphRefs = new MetricGraph?[5];

        private DateTime? lastUpdateUtc;
        private int graphicsCount = 0;

        // ---- view helpers used by the razor ----
        private bool Playing => live?.IsPlaying ?? false;

        private string lastUpdate
        {
            get
            {
                if (lastUpdateUtc == null) return "never";
                var s = (int)(DateTime.UtcNow - lastUpdateUtc.Value).TotalSeconds;
                return s <= 1 ? "1s ago" : $"{s}s ago";
            }
        }

        private string PushedCountdown => Countdown(pushedList?.NextRun);
        private string DailyCountdown => Countdown(dailyList?.NextRun);
        private string NextItemCountdown => Countdown(nextItem?.NextRun);
        private string NextItemLabel =>
            nextItem == null ? "Next In" : $"Next {PlayoutHelper.GetScheduleItemName(nextItem.ItemType)} In";

        private static string Countdown(DateTime? target)
        {
            if (target == null) return "00:00:00:00";
            var r = target.Value - DateTime.Now;
            if (r <= TimeSpan.Zero) return "00:00:00:00";
            return $"{r.Days:00}:{r.Hours:00}:{r.Minutes:00}:{r.Seconds:00}";
        }

        protected override async Task OnParametersSetAsync()
        {
            // Re-init when navigating between playouts (Id changes).
            isLoading = true;
            await ResolvePlayoutMeta();
            await LoadSettings();
            await LoadRestData();

            if (MockFeed.Enabled)
            {
                MockFeed.Register(new[] { Id });
                MockFeed.Start();
            }

            isLoading = false;
        }

        protected override void OnInitialized()
        {
            SignalRService.CommandReceived += OnCommandReceived;
            MockFeed.Tick += OnMockTick;
        }

        private void OnMockTick(string id, PlaylistInfoMessageDto dto)
        {
            if (id != Id) return;
            InvokeAsync(() => ApplyLive(dto));
        }

        private async Task ResolvePlayoutMeta()
        {
            try
            {
                var user = await AuthService.GetCurrentUserAsync();
                var playout = user?.AllPlayouts?.FirstOrDefault(p => p.Id == Id);
                if (playout != null)
                {
                    PlayoutName = playout.Name ?? "Playout";
                    // Client confirmed: PlayoutListDto.Channel IS the channel id.
                    ChannelId = playout.Channel ?? string.Empty;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SinglePlayout] ResolvePlayoutMeta failed: {ex.Message}");
            }
        }

        private async Task LoadSettings()
        {
            scte35Selected = await Settings.GetScte35SelectedAsync(Id);
            for (int slot = 0; slot < 5; slot++)
                graphTypes[slot] = await Settings.GetGraphTypeAsync(Id, slot);
        }

        private async Task LoadRestData()
        {
            errors = await PlayoutApi.GetErrorsLastWeekAsync(Id);
            outputs = await PlayoutApi.GetOutputsAsync(Id);
            eventLogs = await PlayoutApi.GetEventLogsAsync(Id);

            if (!string.IsNullOrEmpty(ChannelId))
            {
                pushedList = await PlayoutApi.GetNextPushedListAsync(ChannelId);
                dailyList = await PlayoutApi.GetNextDailyListAsync(ChannelId);
                nextItem = await PlayoutApi.GetNextScheduleItemAsync(ChannelId);
            }
        }

        // -------- SignalR --------

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
                    if (dto == null) return;

                    // The message belongs to this page if either the envelope's clientId
                    // or the payload's PlayoutId matches the selected playout.
                    var isMine = command.clientId == Id || dto.PlayoutId == Id;
                    Console.WriteLine($"[SinglePlayout] PlayoutStatus recv - clientId='{command.clientId}' dto.PlayoutId='{dto.PlayoutId}' pageId='{Id}' -> {(isMine ? "ACCEPTED" : "ignored")}");
                    if (!isMine) return;

                    InvokeAsync(() => ApplyLive(dto));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SinglePlayout] PlayoutStatus parse failed: {ex.Message}");
                }
            }
            else if (command.command == ServiceMessages.ClientStatusChanged.ToString())
            {
                if (command.clientId == Id || command.playoutId == Id)
                {
                    IsOnline = command.data?.ToString() == "1";
                    Console.WriteLine($"[SinglePlayout] ClientStatusChanged for {Id} -> online={IsOnline}");
                    InvokeAsync(StateHasChanged);
                }
            }
        }

        private void ApplyLive(PlaylistInfoMessageDto dto)
        {
            live = dto;
            IsOnline = true;
            lastUpdateUtc = DateTime.UtcNow;
            foreach (var g in graphRefs)
                g?.Push(dto);
            StateHasChanged();
        }

        // -------- Actions --------

        private async Task StartStopOutput(StartStopOuputDto dto)
        {
            await SignalRService.SendStartStopOutputAsync(dto);
        }

        private async Task ToggleScte()
        {
            scte35Selected = !scte35Selected;
            await Settings.SetScte35SelectedAsync(Id, scte35Selected);
        }

        private async Task OnGraphTypeChanged(int slot, int type)
        {
            graphTypes[slot] = type;
            await Settings.SetGraphTypeAsync(Id, slot, type);
        }

        private async Task ReloadPushed()
        {
            if (!string.IsNullOrEmpty(ChannelId))
            {
                pushedList = await PlayoutApi.GetNextPushedListAsync(ChannelId);
                StateHasChanged();
            }
        }

        private async Task ReloadDaily()
        {
            if (!string.IsNullOrEmpty(ChannelId))
            {
                dailyList = await PlayoutApi.GetNextDailyListAsync(ChannelId);
                StateHasChanged();
            }
        }

        private async Task ReloadNextItem()
        {
            if (!string.IsNullOrEmpty(ChannelId))
            {
                nextItem = await PlayoutApi.GetNextScheduleItemAsync(ChannelId);
                StateHasChanged();
            }
        }

        private void GoToAllPlayouts() => Navigation.NavigateTo("all-playouts");

        public void Dispose()
        {
            SignalRService.CommandReceived -= OnCommandReceived;
            MockFeed.Tick -= OnMockTick;
        }
    }
}
