using System.ComponentModel;

namespace JustBroadcast.Models
{
    public enum OutputTypes
    {
        [Description("SDI")] SDI = 1,
        [Description("NDI")] NDI = 2,
        [Description("DVB UDP")] DVBUDP = 3,
        [Description("DVB SRT")] DVBSRT = 4,
        [Description("HLS")] HLS = 5,
        [Description("HLS SCTE")] HLSSCTE = 6,
        [Description("DASH")] DASH = 7,
        [Description("RTMP")] RTMP = 8,
        [Description("RTMPS")] RTMPS = 9,
        [Description("UDP")] UDP = 10,
        [Description("SRT")] SRT = 11,
        [Description("MPEG TS")] MPEGTS = 12,
        [Description("RTP")] RTP = 13,
        [Description("MPEG-TS RTP")] MPEGTSRTP = 14,
        [Description("RIST")] RIST = 15,
        [Description("RTSP")] RTSP = 16,
        [Description("IIS")] IIS = 17,
        [Description("DShow")] DShow = 18,
        [Description("HDMI")] HDMI = 19
    }
}
