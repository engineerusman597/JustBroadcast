using Blazored.LocalStorage;

namespace JustBroadcast.Services
{
    public class PlayoutSettingsService(ILocalStorageService localStorage) : IPlayoutSettingsService
    {
        private static string ScteKey(string playoutId) => $"playout:{playoutId}:scte35";
        private static string GraphKey(string playoutId, int slot) => $"playout:{playoutId}:graph:{slot}";

        public async Task<bool> GetScte35SelectedAsync(string playoutId)
        {
            try
            {
                if (await localStorage.ContainKeyAsync(ScteKey(playoutId)))
                    return await localStorage.GetItemAsync<bool>(ScteKey(playoutId));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PlayoutSettings] read scte failed: {ex.Message}");
            }
            return true; // default: SCTE 35
        }

        public async Task SetScte35SelectedAsync(string playoutId, bool scte35Selected)
        {
            try
            {
                await localStorage.SetItemAsync(ScteKey(playoutId), scte35Selected);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PlayoutSettings] write scte failed: {ex.Message}");
            }
        }

        public async Task<int> GetGraphTypeAsync(string playoutId, int slot)
        {
            try
            {
                if (await localStorage.ContainKeyAsync(GraphKey(playoutId, slot)))
                    return await localStorage.GetItemAsync<int>(GraphKey(playoutId, slot));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PlayoutSettings] read graph failed: {ex.Message}");
            }
            return slot; // defaults: slot 0->0, 1->1, 2->2, 3->3, 4->4
        }

        public async Task SetGraphTypeAsync(string playoutId, int slot, int chartType)
        {
            try
            {
                await localStorage.SetItemAsync(GraphKey(playoutId, slot), chartType);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PlayoutSettings] write graph failed: {ex.Message}");
            }
        }
    }
}
