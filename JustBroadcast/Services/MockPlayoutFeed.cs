using JustBroadcast.Models;

namespace JustBroadcast.Services
{
    public interface IMockPlayoutFeed
    {
        bool Enabled { get; }
        // Raised ~2x/sec per registered playout with a fresh PlaylistInfoMessageDto.
        event Action<string, PlaylistInfoMessageDto>? Tick;
        void Register(IEnumerable<string> playoutIds);
        void Start();
    }

    // Dev-only live data generator that mimics the SignalR PlayoutStatus stream.
    public class MockPlayoutFeed : IMockPlayoutFeed, IDisposable
    {
        private readonly bool _enabled;
        private readonly List<string> _ids = new();
        private readonly Random _rnd = new();
        private System.Threading.Timer? _timer;
        private double _phase;

        public MockPlayoutFeed(IConfiguration configuration)
        {
            _enabled = Convert.ToBoolean(configuration["ApiSettings:UseMockPlayoutData"]);
        }

        public bool Enabled => _enabled;
        public event Action<string, PlaylistInfoMessageDto>? Tick;

        public void Register(IEnumerable<string> playoutIds)
        {
            foreach (var id in playoutIds)
                if (!_ids.Contains(id)) _ids.Add(id);
        }

        public void Start()
        {
            if (!_enabled || _timer != null) return;
            _timer = new System.Threading.Timer(_ => Emit(), null, 300, 500);
        }

        private void Emit()
        {
            _phase += 0.15;
            for (int i = 0; i < _ids.Count; i++)
            {
                var id = _ids[i];
                var dto = Build(id, i);
                Tick?.Invoke(id, dto);
            }
        }

        private PlaylistInfoMessageDto Build(string id, int index)
        {
            double t = _phase + index;
            // Audio levels are 0..600 (see VuMeter); keep them in a realistic
            // programme range so the meter sits around the green/yellow bands.
            int vuL = (int)(360 + 210 * Math.Abs(Math.Sin(t)));
            int vuR = (int)(360 + 210 * Math.Abs(Math.Sin(t + 0.7)));
            double avgFps = 49.7 + Math.Sin(t) * 0.4;
            double progress = (DateTime.Now.Second / 60.0) * 100.0;
            double fileTime = DateTime.Now.Second + DateTime.Now.Minute * 60;

            return new PlaylistInfoMessageDto
            {
                PlayoutId = id,
                HealthStatus = HealthStatus.Healthy,
                Frame = MockPlayoutData.SampleFrameBase64,
                Audio = new[] { vuL, vuR },
                IsPlaying = true,
                Progress = (float)progress,
                FileTime = fileTime,
                Remaining = 300 - (fileTime % 300),
                Countdown = 300 - (fileTime % 300),
                FPS = avgFps.ToString("0.00"),
                AverageFps = avgFps.ToString("0.00"),
                DesiredFps = "50.00",
                Jitter = (1.5 + Math.Abs(Math.Sin(t)) * 2).ToString("0.00"),
                AvSync = (Math.Sin(t) * 5).ToString("0.00"),
                InputAvg = (18 + Math.Sin(t) * 3).ToString("0.00"),
                Breaks = _rnd.Next(0, 3),
                DroppedFrames = _rnd.Next(0, 2),
                Scte35 = 12 + (int)(t) % 5,
                Scte104 = 3 + (int)(t) % 4,
                Runtime = "0:01:23:45",
                LiveFPS = (50 + Math.Sin(t) * 0.3).ToString("0.00"),
                UrlFPS = (49 + Math.Cos(t) * 0.5).ToString("0.00"),
                InputRecFPS = "50.00",
                OutputRecFPS = "50.00",
                CurrentlyPlayingItemName = "TEST NAME",
                StartTime = "15:22:54",
                PlaylistCount = "24",
                PlaylistRemaining = "17",
                PlaylistDuration = "02:14:30",
                Outputs = new List<OutputInfoMessageDto>
                {
                    Out($"{id}-o1", true, HealthStatus.Healthy, avgFps),
                    Out($"{id}-o2", true, HealthStatus.Warning, avgFps - 1),
                    Out($"{id}-o3", false, HealthStatus.Stopped, 0),
                }
            };
        }

        private OutputInfoMessageDto Out(string outId, bool started, HealthStatus h, double fps) => new()
        {
            OutputId = outId,
            IsStarted = started,
            HealthStatus = h,
            FPS = fps.ToString("0.00"),
            Resets = _rnd.Next(0, 4),
            Jitter = (1 + _rnd.NextDouble()).ToString("0.00"),
            TcBreaks = _rnd.Next(0, 2).ToString()
        };

        public void Dispose() => _timer?.Dispose();
    }
}
