using System.Collections.Generic;

namespace OpenSage.Scripting
{
    public sealed class CounterCollection
    {
        private readonly Dictionary<string, int> _counters;

        public CounterCollection()
        {
            _counters = new Dictionary<string, int>();
        }

        public int this[string name]
        {
            get
            {
                if (!_counters.TryGetValue(name, out var value))
                {
                    return _counters[name] = 0;
                }

                return value;
            }
            set => _counters[name] = value;
        }
    }
}