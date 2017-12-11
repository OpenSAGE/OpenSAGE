using System;
using System.Linq;
using System.Numerics;
using LL.Input;
using OpenSage.Data.Ini;
using OpenSage.Input.Providers;

namespace OpenSage.Input
{
    public sealed class InputSystem : GameSystem
    {
        private readonly InputSettings _settings;
        private readonly InputState _state;

        /// <summary>
        /// Gets or sets the current input provider. Only needs to be changed if <see cref="Game"/>
        /// is in a custom host, such as a scene editor.
        /// </summary>
        public IInputProvider InputProvider { get; set; }

        public bool AnyKeyOrMouseButtonDown => _state.AnyKeyOrMouseButtonPressed;

        public bool AnyMouseButtonDown => _state.AnyMouseButtonPressed;

        public Vector2 MousePosition => new Vector2(_state.CurrentMouseState.X, _state.CurrentMouseState.Y);

        public InputSystem(Game game)
            : base(game)
        {
            _settings = game.Settings.InputSettings;
            _state = new InputState();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            _state.Update(InputProvider);
        }

        public bool GetKeyPressed(Key key)
        {
            return _state.CurrentKeyboardState.IsKeyDown(key) && _state.LastKeyboardState.IsKeyUp(key);
        }

        public bool GetKeyReleased(Key key)
        {
            return _state.CurrentKeyboardState.IsKeyUp(key) && _state.LastKeyboardState.IsKeyDown(key);
        }

        public bool GetKeyDown(Key key)
        {
            return _state.CurrentKeyboardState.IsKeyDown(key);
        }

        public bool GetMouseButtonPressed(MouseButton button)
        {
            return InputUtility.IsMouseButtonPressed(_state.CurrentMouseState, button)
                   && !InputUtility.IsMouseButtonPressed(_state.LastMouseState, button);
        }

        public bool GetMouseButtonReleased(MouseButton button)
        {
            return !InputUtility.IsMouseButtonPressed(_state.CurrentMouseState, button)
                   && InputUtility.IsMouseButtonPressed(_state.LastMouseState, button);
        }

        public bool GetMouseButtonDown(MouseButton button)
        {
            return InputUtility.IsMouseButtonPressed(_state.CurrentMouseState, button);
        }

        public float GetAxis(string axisName)
        {
            var axis = _settings.Axes.FirstOrDefault(a => a.Name == axisName);
            if (axis == null)
                throw new ArgumentException("Could not find an input axis with name '" + axisName + "'.");

            return axis.GetValue(_state);
        }

        public float GetAxis(Key positive, Key negative)
        {
            return new InputAxis
            {
                Mappings =
                {
                    new InputAxisMapping(new KeyboardInputKey(positive), 1.0f),
                    new InputAxisMapping(new KeyboardInputKey(negative), -1.0f)
                }
            }.GetValue(_state);
        }

        public float GetAxis(MouseMovementAxis axis)
        {
            return new MouseMovementInputKey
            {
                Axis = axis
            }.GetAxisValue(_state);
        }

        public bool GetAction(string actionName, InputActionType type)
        {
            var action = _settings.Actions.FirstOrDefault(a => a.Name == actionName);
            if (action == null)
                throw new ArgumentException("Could not find an input action with name '" + actionName + "'.");

            switch (type)
            {
                case InputActionType.Pressed:
                    return action.GetPressed(_state);
                case InputActionType.Released:
                    return action.GetReleased(_state);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
