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

            CurrentGameTime = new GameTime(
                TimeSpan.FromTicks(now - _startTime),
                TimeSpan.FromTicks(deltaTime));
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

        public GameTime(in TimeSpan totalGameTime, in TimeSpan elapsedGameTime)
        {
            TotalGameTime = totalGameTime;
            ElapsedGameTime = elapsedGameTime;
        }
    }
}
