#region File Description
//-----------------------------------------------------------------------------
// Copyright 2011, Nick Gravelyn.
// Licensed under the terms of the Ms-PL:
// http://www.microsoft.com/opensource/licenses.mspx#Ms-PL
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Windows;
using System.Windows.Input;

namespace LLGfx.Hosting
{
    public class HwndMouseEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the state of the left mouse button.
        /// </summary>
        public MouseButtonState LeftButton { get; private set; }

        /// <summary>
        /// Gets the state of the right mouse button.
        /// </summary>
        public MouseButtonState RightButton { get; private set; }

        /// <summary>
        /// Gets the state of the middle mouse button.
        /// </summary>
        public MouseButtonState MiddleButton { get; private set; }

        /// <summary>
        /// Gets the state of the first extra mouse button.
        /// </summary>
        public MouseButtonState X1Button { get; private set; }

        /// <summary>
        /// Gets the state of the second extra mouse button.
        /// </summary>
        public MouseButtonState X2Button { get; private set; }

        /// <summary>
        /// Gets the button that was double clicked.
        /// </summary>
        public MouseButton? DoubleClickButton { get; private set; }

        /// <summary>
        /// Gets the mouse wheel delta.
        /// </summary>
        public int WheelDelta { get; private set; }

        /// <summary>
        /// Gets the horizontal mouse wheel delta.
        /// </summary>
        public int HorizontalWheelDelta { get; private set; }

        /// <summary>
        /// Gets the position of the mouse in screen coordinates.
        /// </summary>
        public Point ScreenPosition { get; private set; }

        /// <summary>
        ///  Calculates the position of the mouse relative to a particular element. 
        /// </summary>
        public Point GetPosition(UIElement relativeTo)
        {
            return relativeTo.PointFromScreen(ScreenPosition);
        }

        /// <summary>
        /// Initializes a new HwndMouseEventArgs.
        /// </summary>
        /// <param name="state">The state from which to initialize the properties.</param>
        public HwndMouseEventArgs(HwndMouseState state)
        {
            LeftButton = state.LeftButton;
            RightButton = state.RightButton;
            MiddleButton = state.MiddleButton;
            X1Button = state.X1Button;
            X2Button = state.X2Button;
            ScreenPosition = state.ScreenPosition;
        }

        /// <summary>
        /// Initializes a new HwndMouseEventArgs.
        /// </summary>
        /// <param name="state">The state from which to initialize the properties.</param>
        /// <param name="mouseWheelDelta">The mouse wheel rotation delta.</param>
        /// <param name="mouseHWheelDelta">The horizontal mouse wheel delta.</param>
        public HwndMouseEventArgs(HwndMouseState state, int mouseWheelDelta, int mouseHWheelDelta)
            : this(state)
        {
            WheelDelta = mouseWheelDelta;
            HorizontalWheelDelta = mouseHWheelDelta;
        }

        /// <summary>
        /// Initializes a new HwndMouseEventArgs.
        /// </summary>
        /// <param name="state">The state from which to initialize the properties.</param>
        /// <param name="doubleClickButton">The button that was double clicked.</param>
        public HwndMouseEventArgs(HwndMouseState state, MouseButton doubleClickButton)
            : this(state)
        {
            DoubleClickButton = doubleClickButton;
        }
    }
}
