namespace JustBroadcast.Models
{
    public class PlayoutListDto
    {
        public required string Id { get; set; }
        public string? Name { get; set; }
        public string? Channel { get; set; }
        public bool? Spare { get; set; }
    }
}
