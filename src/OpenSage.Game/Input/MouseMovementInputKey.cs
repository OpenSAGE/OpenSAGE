using System;

namespace OpenSage.Input
{
    /// <summary>
    /// Configures a mouse movement axis.
    /// </summary>
    public sealed class MouseMovementInputKey : InputKey
    {
        /// <summary>
        /// Gets or sets the mouse axis that will trigger this input key.
        /// </summary>
        public MouseMovementAxis Axis { get; set; }

        /// <summary>
        /// Creates a new <see cref="MouseMovementInputKey"/> with the specified axis.
        /// </summary>
        public MouseMovementInputKey(MouseMovementAxis axis)
        {
            Axis = axis;
        }

        /// <summary>
        /// Creates a new <see cref="MouseMovementInputKey"/>.
        /// </summary>
        public MouseMovementInputKey()
        {
        }

        internal override float GetAxisValue(InputState state)
        {
            switch (Axis)
            {
                case MouseMovementAxis.XAxis:
                    return state.CurrentMouseState.X - state.LastMouseState.X;
                case MouseMovementAxis.YAxis:
                    return state.CurrentMouseState.Y - state.LastMouseState.Y;
                case MouseMovementAxis.ThirdAxis:
                    return state.CurrentMouseState.ScrollWheelValue;
                default:
                    throw new NotSupportedException();
            }
        }

        internal override bool GetPressed(InputState state)
        {
            switch (Axis)
            {
                case MouseMovementAxis.XAxis:
                    return state.CurrentMouseState.X != 0.0f && state.LastMouseState.X == 0.0f;
                case MouseMovementAxis.YAxis:
                    return state.CurrentMouseState.Y != 0.0f && state.LastMouseState.Y == 0.0f;
                case MouseMovementAxis.ThirdAxis:
                    return state.CurrentMouseState.ScrollWheelValue != 0 && state.LastMouseState.ScrollWheelValue == 0;
                default:
                    throw new NotSupportedException();
            }
        }

        internal override bool GetReleased(InputState state)
        {
            switch (Axis)
            {
                case MouseMovementAxis.XAxis:
                    return state.CurrentMouseState.X == 0.0f && state.LastMouseState.X != 0.0f;
                case MouseMovementAxis.YAxis:
                    return state.CurrentMouseState.Y == 0.0f && state.LastMouseState.Y != 0.0f;
                case MouseMovementAxis.ThirdAxis:
                    return state.CurrentMouseState.ScrollWheelValue == 0 && state.LastMouseState.ScrollWheelValue != 0;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}