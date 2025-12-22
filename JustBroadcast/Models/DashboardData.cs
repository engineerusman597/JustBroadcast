namespace JustBroadcast.Models
{
    public class DashboardData
    {
        public PlayoutStats PlayoutStats { get; set; } = new();
        public ChannelStats ChannelStats { get; set; } = new();
        public UserStats UserStats { get; set; } = new();
        public MediaAssetStats MediaAssetStats { get; set; } = new();
        public AlertStats AlertStats { get; set; } = new();
        public List<ActivePlayout> ActivePlayouts { get; set; } = [];
        public List<ActiveUser> ActiveUsers { get; set; } = [];
        public SystemResources SystemResources { get; set; } = new();
        public List<ErrorItem> ErrorFeed { get; set; } = [];
        public List<AlertItem> Alerts { get; set; } = [];
        public Dictionary<string, int> ErrorFrequency { get; set; } = [];
    }

    public class PlayoutStats
    {
        public int Total { get; set; }
        public int Running { get; set; }
        public int Offline { get; set; }
    }

    public class ChannelStats
    {
        public int Total { get; set; }
        public int Assigned { get; set; }
    }

    public class UserStats
    {
        public int Total { get; set; }
        public int Online { get; set; }
    }

    public class MediaAssetStats
    {
        public int Total { get; set; }
        public int AddedThisWeek { get; set; }
    }

    public class AlertStats
    {
        public int Total { get; set; }
        public int RequireAttention { get; set; }
    }

    public class ActivePlayout
    {
        public string Id { get; set; } = string.Empty;
        public string PlayoutName { get; set; } = string.Empty;
        public string ChannelName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Playing { get; set; } = string.Empty;
        public bool Spare { get; set; }
        public int IsOnline { get; set; } = 0; // 0=OFFLINE, 1=ON AIR, 2=UNKNOWN
        public int IsPlaying { get; set; } = 0; // 0=Stopped, 1=Playing
    }

    public class SystemResources
    {
        public int CpuUsage { get; set; }
        public int GpuUsage { get; set; }
        public int RamUsage { get; set; }
        public List<CpuDataPoint> CpuHistory { get; set; } = new();
    }

    public class CpuDataPoint
    {
        public DateTime Time { get; set; }
        public int Value { get; set; }
    }

    public class ErrorItem
    {
        public string Message { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // error, warning
    }

    public class AlertItem
    {
        public string Message { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // error, warning, info
    }

    public class ActiveUser
    {
        public string Name { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string LastSeen { get; set; } = string.Empty;
        public string RoleBadge { get; set; } = string.Empty; // JB1, JB2, etc.
    }
}
