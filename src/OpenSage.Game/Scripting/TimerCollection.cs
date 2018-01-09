using System.Collections.Generic;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Scripting
{
    public sealed class TimerCollection
    {
        private readonly CounterCollection _counters;
        private readonly Dictionary<string, TimerState> _timers;

        private readonly List<string> _expiredTimers;

        public TimerCollection(CounterCollection counters)
        {
            _counters = counters;
            _timers = new Dictionary<string, TimerState>();
            _expiredTimers = new List<string>();
        }

        public void Update()
        {
            foreach (var (timer, state) in _timers)
            {
                var counterValue = _counters[timer];

                if (counterValue < 0)
                {
                    _expiredTimers.Add(timer);
                }
                else if (state == TimerState.Running)
                {
                    _counters[timer] = counterValue - 1;
                }
            }

            foreach (var timer in _expiredTimers)
            {
                _timers.Remove(timer);
            }

            _expiredTimers.Clear();
        }

        public void StartTimer(string name)
        {
            _timers[name] = TimerState.Running;
        }

        public void PauseTimer(string name)
        {
            _timers[name] = TimerState.Paused;
        }

        public bool IsTimerExpired(string name) => _counters[name] < 0;

        private enum TimerState
        {
            Running,
            Paused
        }
    }
}
