using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Input
{
    public struct KeyboardState
    {
        public List<Key> Keys { get; }
        public List<KeyModifiers> KeyModifiers { get; }

        public KeyboardState(List<Key> keys, List<KeyModifiers> keyModifiers)
        {
            Keys = keys;
            KeyModifiers = keyModifiers;
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
