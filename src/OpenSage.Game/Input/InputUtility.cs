using System;

namespace OpenSage.Input
{
    internal static class InputUtility
    {
        public static bool IsMouseButtonPressed(MouseState mouseState, MouseButton mousebuttons)
        {
            switch (mousebuttons)
            {
                case MouseButton.Left:
                    return IsButtonPressed(mouseState.LeftButton);
                case MouseButton.Right:
                    return IsButtonPressed(mouseState.RightButton);
                case MouseButton.Middle:
                    return IsButtonPressed(mouseState.MiddleButton);
                default:
                    throw new NotSupportedException();
            }
        }

        private static bool IsButtonPressed(ButtonState button)
        {
            return button == ButtonState.Pressed;
        }
    }
}