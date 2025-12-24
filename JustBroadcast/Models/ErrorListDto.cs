namespace JustBroadcast.Models
{
    public class ErrorListDto
    {
        public int Type { get; set; } // 0=error, 1=warning, 2=information
        public DateTime Time { get; set; }
        public string Description { get; set; } = string.Empty;
        public string PlayoutName { get; set; } = string.Empty;
    }
}
