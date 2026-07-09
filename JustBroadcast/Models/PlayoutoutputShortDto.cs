namespace JustBroadcast.Models
{
    // GET Playoutoutputs/playout/{id} — one output row for the Outputs grid.
    public class PlayoutoutputShortDto
    {
        public string Id { get; set; } = null!;
        public string PlayoutId { get; set; } = null!;
        public string? VideoFormat { get; set; }
        public string? VideoFormatName { get; set; }
        public string? AudioFormat { get; set; }
        public string? AudioFormatName { get; set; }
        public bool? CustomVideoFormat { get; set; }
        public string? OutputType { get; set; }
        public string? OutputName { get; set; }
        public string? HardwareOutput { get; set; }
        public string? HardwareOutputLineOut { get; set; }
        public string? Url { get; set; }
        public string? Port { get; set; }
    }
}
