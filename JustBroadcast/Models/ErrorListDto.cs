namespace JustBroadcast.Models
{
    public class ErrorListDto
    {
        public string? Id { get; set; }
        public int Type { get; set; } // 0=error, 1=warning, 2=information
        public DateTime Time { get; set; }
        public string Description { get; set; } = string.Empty;
        public string PlayoutName { get; set; } = string.Empty;
        public string? ToDo { get; set; }
        public int Status { get; set; }
    }
}
