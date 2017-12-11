using System;

namespace LL.Input
{
    public sealed class MouseEventArgs : EventArgs
    {
        /// <summary>
        /// Gets which mouse button this event was triggered by.
        /// </summary>
        public MouseButton Button { get; }

        /// <summary>
        /// Gets the number of times the mouse button was clicked.
        /// </summary>
        public int ClickCount { get; }

        /// <summary>
        /// Gets the mouse wheel delta.
        /// </summary>
        public int WheelDelta { get; }

        /// <summary>
        /// Gets the X position of the mouse in local control coordinates.
        /// </summary>
        public int PositionX { get; }

        /// <summary>
        /// Gets the Y position of the mouse in local control coordinates.
        /// </summary>
        public int PositionY { get; }

        public MouseEventArgs(MouseButton button, int clickCount, int wheelDelta, int x, int y)
        {
            Button = button;
            ClickCount = clickCount;
            WheelDelta = wheelDelta;
            PositionX = x;
            PositionY = y;
        }
    }
}
