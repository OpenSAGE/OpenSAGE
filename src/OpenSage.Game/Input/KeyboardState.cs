using System.Collections.Generic;
using LL.Input;

namespace OpenSage.Input
{
    public struct KeyboardState
    {
        public List<Key> Keys { get; }

        public KeyboardState(List<Key> keys)
        {
            Keys = keys;
        }

        public bool IsKeyDown(Key key)
        {
            return Keys.Contains(key);
        }

        public bool IsKeyUp(Key key)
        {
            return !Keys.Contains(key);
        }
    }
}
