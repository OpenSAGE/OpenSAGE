namespace OpenSage.Input
{
    /// <summary>
    /// Base class for input keys, used to configure <see cref="InputActionMapping"/> and
    /// <see cref="InputAxisMapping"/>. "Key" is a misnomer; inherited classes are used
    /// for keyboard keys, mouse buttons, mouse movement, gamepad buttons, 
    /// gamepad thumbsticks, and gamepad triggers.
    /// </summary>
    public abstract class InputKey
    {
        internal abstract float GetAxisValue(InputState state);
        internal abstract bool GetPressed(InputState state);
        internal abstract bool GetReleased(InputState state);
    }
}