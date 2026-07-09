namespace JustBroadcast.Models
{
    // GET Eventlogs/20/playout/{id} — event log rows for the Event Log panel.
    public class EventlogDto
    {
        public string Id { get; set; } = null!;
        public string PlayoutId { get; set; } = null!;
        public int? LogType { get; set; }
        public DateTime? Time { get; set; }
        public string? Description { get; set; }
        public int? ItemType { get; set; }
    }
}
