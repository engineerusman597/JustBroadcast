namespace JustBroadcast.Models
{
    // Sent per playout via ServiceMessages.PlayoutStatus (SignalR).
    public class PlaylistInfoMessageDto
    {
        public string? PlayoutId { get; set; }
        public HealthStatus HealthStatus { get; set; } = HealthStatus.Unknown;
        public object? Frame { get; set; }
        public double? Countdown { get; set; }
        public float? Progress { get; set; }
        public string? FPS { get; set; }
        public bool IsPlaying { get; set; }
        public double FileTime { get; set; }
        public double Remaining { get; set; }

        public int[]? Audio { get; set; }
        public string Jitter { get; set; } = "0.00";
        public int Breaks { get; set; }
        public int DroppedFrames { get; set; }
        public string AvSync { get; set; } = "0.00";

        public string Runtime { get; set; } = "0:00:00:00";
        public string DesiredFps { get; set; } = "0.00";
        public string AverageFps { get; set; } = "0.00";

        public string InputAvg { get; set; } = "0.00";
        public int Scte104 { get; set; }
        public int Scte35 { get; set; }

        public HealthStatus LiveHealthStatus { get; set; } = HealthStatus.Unknown;
        public string? LiveFPS { get; set; }

        public HealthStatus UrlHealthStatus { get; set; } = HealthStatus.Unknown;
        public string? UrlFPS { get; set; }

        public HealthStatus InputRecHealthStatus { get; set; } = HealthStatus.Unknown;
        public string? InputRecFPS { get; set; }

        public HealthStatus OutputRecHealthStatus { get; set; } = HealthStatus.Unknown;
        public string? OutputRecFPS { get; set; }

        public List<OutputInfoMessageDto>? Outputs { get; set; }

        public string CurrentlyPlayingItemName { get; set; } = "0";
        public string StartTime { get; set; } = "00:00:00";
        public string PlaylistCount { get; set; } = "0";
        public string PlaylistRemaining { get; set; } = "0";
        public string PlaylistDuration { get; set; } = "00:00:00";
    }
}
