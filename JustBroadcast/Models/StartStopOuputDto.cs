namespace JustBroadcast.Models
{
    // Sent client -> server via ServiceMessages.StartStopOutput to start/stop an output.
    // When stopping a started output, send IsStarted = !current (i.e. false).
    public class StartStopOuputDto
    {
        public string? PlayoutId { get; set; }
        public string? OutputId { get; set; }
        public bool? IsStarted { get; set; }
    }
}
