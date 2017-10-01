namespace OpenSage.Input
{
    /// <summary>
    /// Stores the <see cref="InputKey"/> and scale for an axis mapping.
    /// </summary>
    public sealed class InputAxisMapping
    {
        /// <summary>
        /// Gets or sets the scale factor that the raw <see cref="Key"/> value will be multiplied by.
        /// </summary>
        public float Scale { get; set; } = 1.0f;

        /// <summary>
        /// Gets or sets the <see cref="InputKey"/> subclass for this axis mapping.
        /// </summary>
        public InputKey Key { get; set; }

        /// <summary>
        /// Creates a new <see cref="InputAxisMapping"/> with the specified <see cref="InputKey"/> and scale factor.
        /// </summary>
        public InputAxisMapping(InputKey key, float scale)
        {
            Key = key;
            Scale = scale;
        }

        /// <summary>
        /// Creates a new <see cref="InputAxisMapping"/>.
        /// </summary>
        public InputAxisMapping()
        {
        }

        internal float GetValue(InputState state)
        {
            if (Key == null)
                return 0.0f;

            return Key.GetAxisValue(state) * Scale;
        }
    }
}