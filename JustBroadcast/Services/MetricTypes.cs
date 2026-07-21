using System.Globalization;
using JustBroadcast.Models;

namespace JustBroadcast.Services
{
    // The seven selectable metrics, shared by the single view's charts (MetricGraph)
    // and the N-up tiles' numeric readouts (MetricValue). Both offer the same list,
    // so labels, colours and DTO field mapping live here rather than being duplicated.
    public static class MetricTypes
    {
        public const int Count = 7;

        public static string Label(int type) => type switch
        {
            0 => "FPS (AVG)",
            1 => "JITTER (INPUT)",
            2 => "INPUT FRAME TIME",
            3 => "LIVE FPS (AVG)",
            4 => "URL FPS (AVG)",
            5 => "INPUT RECORDER FPS (AVG)",
            6 => "OUTPUT RECORDER FPS (AVG)",
            _ => ""
        };

        // Shorter labels for the small tiles, where the full text doesn't fit.
        public static string ShortLabel(int type) => type switch
        {
            0 => "FPS",
            1 => "JITTER",
            2 => "FRAME TIME",
            3 => "LIVE FPS",
            4 => "URL FPS",
            5 => "IN REC FPS",
            6 => "OUT REC FPS",
            _ => ""
        };

        public static string Color(int type) => type switch
        {
            0 => "#5ecb16",
            1 => "#0098df",
            2 => "#f7db07",
            3 => "#970fe1",
            4 => "#02e1e8",
            5 => "#ff7800",
            6 => "#ff4081",
            _ => "#5ecb16"
        };

        // Unit shown after the value; empty for plain frame rates.
        public static string Suffix(int type) => type switch
        {
            1 or 2 => " ms",
            _ => ""
        };

        // The raw string field this metric plots/displays.
        public static string? RawValue(int type, PlaylistInfoMessageDto dto) => type switch
        {
            0 => dto.AverageFps,
            1 => dto.Jitter,
            2 => dto.InputAvg,
            3 => dto.LiveFPS,
            4 => dto.UrlFPS,
            5 => dto.InputRecFPS,
            6 => dto.OutputRecFPS,
            _ => null
        };

        // The text shown as the "current" reading. FPS prefers the instantaneous
        // FPS field and falls back to the average when it isn't present.
        public static string DisplayValue(int type, PlaylistInfoMessageDto dto)
        {
            if (type == 0)
                return dto.FPS ?? dto.AverageFps ?? "—";

            var raw = RawValue(type, dto);
            if (string.IsNullOrEmpty(raw))
                return type is 1 or 2 ? "0" + Suffix(type) : "—";

            return raw + Suffix(type);
        }

        public static bool TryParse(string? raw, out double value) =>
            double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
    }
}
