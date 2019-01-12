using System;
using System.Diagnostics;

namespace OpenSage
{
    public sealed class GameTimer : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private long _startTime;
        private long _lastUpdate;

        public GameTime CurrentGameTime { get; private set; }

        public GameTimer()
        {
            _stopwatch = new Stopwatch();
        }

        private long GetTimeNow() => _stopwatch.ElapsedTicks;

        public void Start()
        {
            _stopwatch.Start();
            Reset();
        }

        public void Update()
        {
            var now = GetTimeNow();
            var deltaTime = now - _lastUpdate;
            _lastUpdate = now;

            CurrentGameTime = new GameTime(now - _startTime, deltaTime);
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

    public readonly struct GameTime
    {
        public readonly TimeSpan TotalGameTime;
        public readonly TimeSpan ElapsedGameTime;

        public GameTime(long totalGameTimeTicks, long elapsedGameTimeTicks)
        {
            TotalGameTime = new TimeSpan( (long)(totalGameTimeTicks * TickRatio));
            ElapsedGameTime = new TimeSpan( (long)(elapsedGameTimeTicks * TickRatio));
        }

        public static GameTime Zero { get; } = new GameTime();

        // This is the ratio between stopwatch ticks and timespan ticks
        private static readonly double TickRatio = 10000000.0 / Stopwatch.Frequency;
    }
}
