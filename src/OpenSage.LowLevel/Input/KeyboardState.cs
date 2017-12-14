using System.Collections.Generic;

namespace LL.Input
{
    public struct KeyboardState
    {
        private readonly List<Key> _keys;

        public IReadOnlyList<Key> Keys => _keys;

        public KeyboardState(List<Key> keys)
        {
            _keys = keys;
        }

        public bool IsKeyDown(Key key)
        {
            return _keys.Contains(key);
        }

        public bool IsKeyUp(Key key)
        {
            return !_keys.Contains(key);
        }
    }
}
