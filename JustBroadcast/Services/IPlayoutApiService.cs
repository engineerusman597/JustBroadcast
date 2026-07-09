using JustBroadcast.Models;

namespace JustBroadcast.Services
{
    public interface IPlayoutApiService
    {
        Task<List<ErrorListDto>> GetErrorsLastWeekAsync(string playoutId);
        Task<List<PlayoutoutputShortDto>> GetOutputsAsync(string playoutId);
        Task<NextScheduleItemDto?> GetNextPushedListAsync(string channelId);
        Task<NextScheduleItemDto?> GetNextDailyListAsync(string channelId);
        Task<NextScheduleItemDto?> GetNextScheduleItemAsync(string channelId);
        Task<List<EventlogDto>> GetEventLogsAsync(string playoutId);
    }
}
