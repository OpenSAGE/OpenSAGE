using System;
using System.Diagnostics;

namespace OpenSage
{
    public sealed class GameTimer : IDisposable
    {
        private readonly Stopwatch _stopwatch;
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
            _lastUpdate = GetTimeNow();
        }

        public void Update()
        {
            var now = GetTimeNow();
            var deltaTime = now - _lastUpdate;
            _lastUpdate = now;

            CurrentGameTime = new GameTime
            {
                ElapsedGameTime = TimeSpan.FromMilliseconds(deltaTime)
            };
        }

        public void Reset()
        {
            _lastUpdate = GetTimeNow();
        }

        public void Dispose()
        {
            _stopwatch.Stop();
        }
    }

    public struct GameTime
    {
        public TimeSpan ElapsedGameTime;
    }
}
