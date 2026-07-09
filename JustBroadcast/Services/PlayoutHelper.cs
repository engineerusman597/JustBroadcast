using System.ComponentModel;
using System.Reflection;
using JustBroadcast.Models;

namespace JustBroadcast.Services
{
    // Shared conversion/formatting logic translated from the WinForms dashboard reference.
    public static class PlayoutHelper
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attr = field?.GetCustomAttribute<DescriptionAttribute>();
            return attr?.Description ?? value.ToString();
        }

        // GetEnumDescription<OutputTypes>(OutputType) -> "SDI", "NDI", ...
        public static string GetEnumDescription<T>(string? value) where T : Enum
        {
            if (!int.TryParse(value, out int intValue))
                return "";

            var enumValue = (T)Enum.ToObject(typeof(T), intValue);
            return (enumValue as Enum)?.GetDescription() ?? "";
        }

        // Double seconds -> "HH:MM:SS" (used for FileTime, Remaining, Countdown).
        public static string Dbl2PosStrNoMiliseconds(double dblPos)
        {
            int nHour = (int)dblPos / 3600;
            int nMinutes = ((int)dblPos % 3600) / 60;
            int nSec = (int)dblPos % 60;
            return $"{nHour:00}:{nMinutes:00}:{nSec:00}";
        }

        // Source column for the Outputs grid / FPS popover.
        public static string SetDeviceOrUrl(PlayoutoutputShortDto? outp)
        {
            if (outp == null) return string.Empty;

            string type = string.Empty;
            switch (outp.OutputType)
            {
                case "1":
                    type = outp.HardwareOutput ?? string.Empty;
                    break;
                case "2":
                    type = !string.IsNullOrEmpty(outp.HardwareOutputLineOut)
                        ? outp.HardwareOutputLineOut!
                        : "NDI Output";
                    break;
                case "3":
                case "4":
                case "10":
                case "11":
                case "12":
                case "13":
                case "14":
                case "15":
                case "16":
                    type = $"{outp.Url}:{outp.Port}";
                    break;
                case "5":
                    type = "HLS";
                    break;
                case "6":
                    type = "HLS SCTE";
                    break;
                case "7":
                    type = "DASH";
                    break;
                case "8":
                case "9":
                case "17":
                    type = outp.Url ?? string.Empty;
                    break;
                case "18":
                    type = "Direct Show Output";
                    break;
                case "19":
                    type = "HDMI Output";
                    break;
            }
            return type;
        }

        // "Next {type} In" label for the Schedule Item panel.
        public static string GetScheduleItemName(string? itemType)
        {
            if (int.TryParse(itemType, out int val) && Enum.IsDefined(typeof(ScheduleItemType), val))
                return ((ScheduleItemType)val).ToString();
            return "Item";
        }

        // ItemType name map for the Event Log panel.
        public static string GetEventLogItemTypeName(int? itemType)
        {
            return itemType switch
            {
                0 => "System",
                1 => "Playout",
                2 => "Playlist",
                3 => "Output",
                _ => ""
            };
        }

        // Health -> (label, hex color) used across outputs and health widgets.
        public static (string Label, string Color) HealthDisplay(HealthStatus status)
        {
            return status switch
            {
                HealthStatus.Healthy => ("HEALTHY", "#5ecb16"),
                HealthStatus.Warning => ("WARNING", "#ffb115"),
                HealthStatus.Critical => ("CRITICAL", "#d11c1c"),
                HealthStatus.Faulted => ("FAULTED", "#d11c1c"),
                HealthStatus.Stopped => ("STOPPED", "#2d79f7"),
                HealthStatus.Restarting => ("RESTARTING", "#ffb115"),
                _ => ("UNKNOWN", "#dcdcdc")
            };
        }

        // FPS string -> "0.00" formatted (truncate to 2 decimals).
        public static string FormatFps(string? fps)
        {
            if (!string.IsNullOrEmpty(fps) &&
                double.TryParse(fps, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var v))
            {
                return (Math.Truncate(v * 100) / 100)
                    .ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
            }
            return "0.00";
        }
    }
}
