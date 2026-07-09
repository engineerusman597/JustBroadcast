namespace JustBroadcast.Models
{
    // Nested list inside PlaylistInfoMessageDto.Outputs (SignalR).
    public class OutputInfoMessageDto
    {
        public string? OutputId { get; set; }
        public bool? IsStarted { get; set; }
        public HealthStatus HealthStatus { get; set; } = HealthStatus.Unknown;
        public string? FPS { get; set; }
        public int Resets { get; set; }
        public string Jitter { get; set; } = "0.00";
        public string TcBreaks { get; set; } = "0";
    }
}
