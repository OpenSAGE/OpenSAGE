using OpenSage.Data.Ini;

namespace OpenSage.Input
{
    /// <summary>
    /// Configures a keyboard key.
    /// </summary>
    public sealed class KeyboardInputKey : InputKey
    {
        /// <summary>
        /// Gets or sets the keyboard key that will trigger this input key.
        /// </summary>
        public Key Key { get; set; }

        /// <summary>
        /// Creates a new <see cref="KeyboardInputKey"/> with the specified keyboard key.
        /// </summary>
        public KeyboardInputKey(Key key)
        {
            Key = key;
        }

        /// <summary>
        /// Creates a new <see cref="KeyboardInputKey"/>.
        /// </summary>
        public KeyboardInputKey()
        {
        }

        internal override float GetAxisValue(InputState state)
        {
            return state.CurrentKeyboardState.IsKeyDown(Key) ? 1.0f : 0.0f;
        }

        internal override bool GetPressed(InputState state)
        {
            return state.CurrentKeyboardState.IsKeyDown(Key) && state.LastKeyboardState.IsKeyUp(Key);
        }

        internal override bool GetReleased(InputState state)
        {
            return state.CurrentKeyboardState.IsKeyUp(Key) && state.LastKeyboardState.IsKeyDown(Key);
        }
    }
}