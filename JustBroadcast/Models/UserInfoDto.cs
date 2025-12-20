namespace JustBroadcast.Models
{
    public class UserInfoDto
    {
        public required string Id { get; set; }
        public string? Username { get; set; }
        public string? Name { get; set; }
        public int? Role { get; set; }
        public byte[]? Picture { get; set; }
        public List<PlayoutListDto>? AllPlayouts { get; set; }
        public List<PlayoutListDto>? RemotePlayouts { get; set; }
        public List<ChannelListDto>? SchedulerChannels { get; set; }
    }
}
