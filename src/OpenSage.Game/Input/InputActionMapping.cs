namespace OpenSage.Input
{
    /// <summary>
    /// Stores the <see cref="InputKey"/> for an action mapping.
    /// </summary>
    public sealed class InputActionMapping
    {
        /// <summary>
        /// Gets or sets the <see cref="InputKey"/> subclass for this action mapping.
        /// </summary>
        public InputKey Key { get; set; }

        /// <summary>
        /// Creates a new <see cref="InputActionMapping"/> with the specified <see cref="InputKey"/>.
        /// </summary>
        /// <param name="key"></param>
        public InputActionMapping(InputKey key)
        {
            Key = key;
        }

        /// <summary>
        /// Creates a new <see cref="InputActionMapping"/>.
        /// </summary>
        public InputActionMapping()
        {
        }

        internal bool GetPressed(InputState state)
        {
            return Key != null && Key.GetPressed(state);
        }

        internal bool GetReleased(InputState state)
        {
            return Key != null && Key.GetReleased(state);
        }
    }
}