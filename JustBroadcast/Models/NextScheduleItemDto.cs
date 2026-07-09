namespace JustBroadcast.Models
{
    // GET Schedulelists/pushed|daily/getnext/channel/{id} and Scheduleitems/channel/next/{id}.
    public class NextScheduleItemDto
    {
        public string ScheduleRuleId { get; set; } = string.Empty;
        public string Name { get; set; } = "";
        public string ItemType { get; set; } = "";
        public string TargetName { get; set; } = "";
        public DateTime NextRun { get; set; }
        public int Priority { get; set; }
        public int TotalCount { get; set; }
    }
}
