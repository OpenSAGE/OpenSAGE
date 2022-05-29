using System;
using System.Diagnostics;

namespace OpenSage
{
    public sealed class DeltaTimer : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private long _startTime;
        private long _lastUpdate;

        public TimeInterval CurrentGameTime { get; private set; }

        public DeltaTimer()
        {
            _stopwatch = new Stopwatch();
        }

        private long GetTimeNow() => _stopwatch.ElapsedTicks;

        public void Start()
        {
            _stopwatch.Start();
            Reset();
        }

        public void Continue()
        {
            _stopwatch.Start();
        }

        public void Pause()
        {
            _stopwatch.Stop();

        }

        public void Update()
        {
            var now = GetTimeNow();
            var deltaTime = now - _lastUpdate;
            _lastUpdate = now;

            CurrentGameTime = new TimeInterval(now - _startTime, deltaTime);
        }

        public void Reset()
        {
            _lastUpdate = GetTimeNow();
            _startTime = _lastUpdate;
        }

        public void Dispose()
        {
            _stopwatch.Stop();
        }
    }

    public readonly struct TimeInterval
    {
        public readonly TimeSpan TotalTime;
        public readonly TimeSpan DeltaTime;

        public TimeInterval(long totalGameTimeTicks, long elapsedGameTimeTicks)
        {
            TotalTime = new TimeSpan((long) (totalGameTimeTicks * TickRatio));
            DeltaTime = new TimeSpan((long) (elapsedGameTimeTicks * TickRatio));
        }

        public TimeInterval(TimeSpan totalTime, TimeSpan deltaTime)
        {
            TotalTime = totalTime;
            DeltaTime = deltaTime;
        }

        public TimeInterval(TimeInterval t) : this(t.TotalTime, t.DeltaTime) { }

        public static TimeInterval Zero { get; } = new TimeInterval();

        // This is the ratio between stopwatch ticks and timespan ticks
        private static readonly double TickRatio = 10000000.0 / Stopwatch.Frequency;
    }
}
