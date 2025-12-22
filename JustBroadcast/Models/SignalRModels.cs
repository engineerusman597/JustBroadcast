namespace JustBroadcast.Models
{
    public class CommandDto
    {
        public string command { get; set; } = string.Empty;
        public string clientId { get; set; } = string.Empty;
        public string group { get; set; } = string.Empty;
        public object? data { get; set; }
    }

    public class PlayoutListInfo
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Channel { get; set; }
        public bool? Spare { get; set; }
        public int IsOnline { get; set; } = 0; // 0=OFFLINE, 1=ON AIR, 2=UNKNOWN
        public int IsPlaying { get; set; } = 0; // 0=Stopped, 1=Playing
    }

    public enum ServiceMessages
    {
        RequestStatusSync,
        ClientStatusChanged
    }

    public enum ClientType
    {
        PlayoutServer,
        RemoteControl,
        Scheduler,
        CgControl,
        WebDashboard
    }
}
