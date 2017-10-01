namespace OpenSage.Input
{
    /// <summary>
    /// Configures a mouse button.
    /// </summary>
    public sealed class MouseButtonInputKey : InputKey
    {
        /// <summary>
        /// Gets or sets the mouse button that will trigger this input key.
        /// </summary>
        public MouseButton Button { get; set; }

        /// <summary>
        /// Creates a new <see cref="MouseButtonInputKey"/> with the specified mouse button.
        /// </summary>
        /// <param name="button"></param>
        public MouseButtonInputKey(MouseButton button)
        {
            Button = button;
        }

        /// <summary>
        /// Creates a new <see cref="MouseButtonInputKey"/>.
        /// </summary>
        public MouseButtonInputKey()
        {
        }

        internal override float GetAxisValue(InputState state)
        {
            return InputUtility.IsMouseButtonPressed(state.CurrentMouseState, Button) ? 1.0f : 0.0f;
        }

        internal override bool GetPressed(InputState state)
        {
            return InputUtility.IsMouseButtonPressed(state.CurrentMouseState, Button)
                   && !InputUtility.IsMouseButtonPressed(state.LastMouseState, Button);
        }

        internal override bool GetReleased(InputState state)
        {
            return !InputUtility.IsMouseButtonPressed(state.CurrentMouseState, Button)
                   && InputUtility.IsMouseButtonPressed(state.LastMouseState, Button);
        }
    }
}