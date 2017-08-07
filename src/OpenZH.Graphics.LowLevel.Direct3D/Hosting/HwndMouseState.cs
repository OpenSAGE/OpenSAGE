#region File Description
//-----------------------------------------------------------------------------
// Copyright 2011, Nick Gravelyn.
// Licensed under the terms of the Ms-PL: 
// http://www.microsoft.com/opensource/licenses.mspx#Ms-PL
//-----------------------------------------------------------------------------
#endregion

using System.Windows;
using System.Windows.Input;

namespace OpenZH.Graphics.LowLevel.Hosting
{
    public class HwndMouseState
    {
        /// <summary>
        /// The current state of the left mouse button.
        /// </summary>
        public MouseButtonState LeftButton;

        /// <summary>
        /// The current state of the right mouse button.
        /// </summary>
        public MouseButtonState RightButton;

        /// <summary>
        /// The current state of the middle mouse button.
        /// </summary>
        public MouseButtonState MiddleButton;

        /// <summary>
        /// The current state of the first extra mouse button.
        /// </summary>
        public MouseButtonState X1Button;

        /// <summary>
        /// The current state of the second extra mouse button.
        /// </summary>
        public MouseButtonState X2Button;

        /// <summary>
        /// The current position of the mouse in screen coordinates.
        /// </summary>
        public Point ScreenPosition;
    }
}
