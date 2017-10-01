namespace OpenSage.Input
{
    /// <summary>
    /// Passed to <see cref="InputSystem.GetAction"/> to control whether the calling
    /// code is interested in a press or release.
    /// </summary>
    public enum InputActionType
    {
        /// <summary>
        /// <see cref="InputSystem.GetAction"/> returns true if any keys mapped to the specified
        /// action have been pressed.
        /// </summary>
        Pressed,

        /// <summary>
        /// <see cref="InputSystem.GetAction"/> returns true if any keys mapped to the specified
        /// action have been released.
        /// </summary>
        Released
    }
}
