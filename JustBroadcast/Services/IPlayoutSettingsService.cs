namespace JustBroadcast.Services
{
    // Per-playout view preferences persisted across sessions (browser localStorage).
    public interface IPlayoutSettingsService
    {
        // SCTE toggle: true = show SCTE 35, false = show SCTE 104. Default true.
        Task<bool> GetScte35SelectedAsync(string playoutId);
        Task SetScte35SelectedAsync(string playoutId, bool scte35Selected);

        // Graph slot 0..4 -> chart type 0..6. Defaults: 0,1,2,3,4.
        Task<int> GetGraphTypeAsync(string playoutId, int slot);
        Task SetGraphTypeAsync(string playoutId, int slot, int chartType);

        // Numeric metric slot 0..4 in the N-up tiles -> metric type 0..6.
        // Stored separately from the graph slots so the two views don't overwrite
        // each other. Defaults: 0,1,2,3,4.
        Task<int> GetMetricTypeAsync(string playoutId, int slot);
        Task SetMetricTypeAsync(string playoutId, int slot, int metricType);
    }
}
