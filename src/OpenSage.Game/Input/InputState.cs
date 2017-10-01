using System.Linq;
using OpenSage.Input.Providers;

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
            || CurrentKeyboardState.KeyModifiers.Any() 
            || AnyMouseButtonPressed;

        public bool AnyMouseButtonPressed => CurrentMouseState.LeftButton == ButtonState.Pressed
                                             || CurrentMouseState.RightButton == ButtonState.Pressed
                                             || CurrentMouseState.MiddleButton == ButtonState.Pressed;

        public void Update(IInputProvider inputProvider)
        {
            LastKeyboardState = CurrentKeyboardState;
            LastMouseState = CurrentMouseState;

            inputProvider.UpdateInputState(this);

            if (!_doneFirstUpdate)
            {
                LastKeyboardState = CurrentKeyboardState;
                LastMouseState = CurrentMouseState;

                _doneFirstUpdate = true;
            }
        }
    }
}
