using System;
using System.Diagnostics;

namespace OpenSage
{
    public sealed class GameTimer : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private double _startTime;
        private double _lastUpdate;

        public GameTime CurrentGameTime { get; private set; }

        public GameTimer()
        {
            _stopwatch = new Stopwatch();
        }

        private double GetTimeNow() => _stopwatch.ElapsedMilliseconds;

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
                TimeSpan.FromMilliseconds(now - _startTime),
                TimeSpan.FromMilliseconds(deltaTime));
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
