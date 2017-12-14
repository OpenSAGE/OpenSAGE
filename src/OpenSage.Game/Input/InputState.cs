using System.Linq;
using LL.Input;

namespace OpenSage.Input
{
    public sealed class InputState
    {
        private bool _doneFirstUpdate;

        public KeyboardState CurrentKeyboardState { get; set; }
        public KeyboardState LastKeyboardState { get; set; }
        public MouseState CurrentMouseState { get; set; }
        public MouseState LastMouseState { get; set; }

        public bool AnyKeyOrMouseButtonPressed => CurrentKeyboardState.Keys.Any() 
            || AnyMouseButtonPressed;

        public bool AnyMouseButtonPressed => CurrentMouseState.LeftButton == ButtonState.Pressed
                                             || CurrentMouseState.RightButton == ButtonState.Pressed
                                             || CurrentMouseState.MiddleButton == ButtonState.Pressed;

        internal void Update(in KeyboardState keyboardState, in MouseState mouseState)
        {
            LastKeyboardState = CurrentKeyboardState;
            LastMouseState = CurrentMouseState;

            CurrentKeyboardState = keyboardState;
            CurrentMouseState = mouseState;

            if (!_doneFirstUpdate)
            {
                LastKeyboardState = CurrentKeyboardState;
                LastMouseState = CurrentMouseState;

                _doneFirstUpdate = true;
            }
        }
    }
}
