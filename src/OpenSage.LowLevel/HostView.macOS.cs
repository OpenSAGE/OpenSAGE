using System;
using System.Collections.Generic;
using AppKit;
using CoreGraphics;
using MetalKit;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.LowLevel.Input;

namespace OpenSage.LowLevel
{
    partial class HostView : MTKView
    {
        #region Graphics

        public SwapChain SwapChain { get; private set; }

        public HostView()
            : base(CGRect.Empty, HostPlatform.GraphicsDevice.Device)
        {
            Paused = false;
            EnableSetNeedsDisplay = false;
        }

        public override void ViewDidMoveToWindow()
        {
            if (Window != null && SwapChain == null)
            {
                SwapChain = new SwapChain(GraphicsDevice, this);

                OnGraphicsInitialized(EventArgs.Empty);
            }

            base.ViewDidMoveToWindow();
        }

        public override void DrawRect(CGRect dirtyRect)
        {
            base.DrawRect(dirtyRect);

            if (SwapChain == null)
            {
                return;
            }

            OnGraphicsDraw(EventArgs.Empty);
        }

        // TODO: Handle size changed.
        // OnGraphicsResized(EventArgs.Empty);

        protected override void Dispose(bool disposing)
        {
            OnGraphicsUninitialized(EventArgs.Empty);

            SwapChain?.Dispose();
            SwapChain = null;

            base.Dispose(disposing);
        }

        #endregion

        #region Input

        private readonly List<Key> _pressedKeys = new List<Key>();

        public override void KeyDown(NSEvent theEvent)
        {
            throw new NotImplementedException();

            //var key = MapKey(e.KeyCode);
            //lock (_pressedKeys)
            //{
            //    if (!_pressedKeys.Contains(key))
            //    {
            //        _pressedKeys.Add(key);
            //    }
            //}

            base.KeyDown(theEvent);
        }

        public override void KeyUp(NSEvent theEvent)
        {
            throw new NotImplementedException();

            //var key = MapKey(e.KeyCode);
            //lock (_pressedKeys)
            //{
            //    _pressedKeys.Remove(key);
            //}

            base.KeyUp(theEvent);
        }

        private KeyboardState PlatformGetKeyboardState()
        {
            lock (_pressedKeys)
            {
                return new KeyboardState(new List<Key>(_pressedKeys));
            }
        }

        private MouseState _mouseState;

        public override void MouseEntered(NSEvent theEvent)
        {
            base.MouseEntered(theEvent);
        }

        public override void MouseExited(NSEvent theEvent)
        {
            base.MouseExited(theEvent);
        }

        public override void MouseDown(NSEvent theEvent)
        {
            SetMousePosition(theEvent.LocationInWindow);
            _mouseState.LeftButton = ButtonState.Pressed;
        }

        public override void MouseUp(NSEvent theEvent)
        {
            SetMousePosition(theEvent.LocationInWindow);
            _mouseState.LeftButton = ButtonState.Released;
        }

        public override void RightMouseDown(NSEvent theEvent)
        {
            SetMousePosition(theEvent.LocationInWindow);
            _mouseState.RightButton = ButtonState.Pressed;
        }

        public override void RightMouseUp(NSEvent theEvent)
        {
            SetMousePosition(theEvent.LocationInWindow);
            _mouseState.RightButton = ButtonState.Released;
        }

        public override void OtherMouseDown(NSEvent theEvent)
        {
            // TODO: theEvent.ButtonNumber
            SetMousePosition(theEvent.LocationInWindow);
            _mouseState.MiddleButton = ButtonState.Pressed;
        }

        public override void OtherMouseUp(NSEvent theEvent)
        {
            // TODO: theEvent.ButtonNumber
            SetMousePosition(theEvent.LocationInWindow);
            _mouseState.MiddleButton = ButtonState.Released;
        }

        public override void MouseMoved(NSEvent theEvent)
        {
            SetMousePosition(theEvent.LocationInWindow);
        }

        public override void MouseDragged(NSEvent theEvent)
        {
            SetMousePosition(theEvent.LocationInWindow);
        }

        public override void RightMouseDragged(NSEvent theEvent)
        {
            SetMousePosition(theEvent.LocationInWindow);
        }

        public override void OtherMouseDragged(NSEvent theEvent)
        {
            SetMousePosition(theEvent.LocationInWindow);
        }

        private void SetMousePosition(CGPoint mouseLocation)
        {
            var controlPosition = ConvertPointFromView(mouseLocation, null);

            _mouseState.X = (int) controlPosition.X;
            _mouseState.Y = (int) controlPosition.Y;
        }

        public override void ScrollWheel(NSEvent theEvent)
        {
            _mouseState.ScrollWheelValue += (int) theEvent.ScrollingDeltaY;
        }

        private MouseState PlatformGetMouseState() => _mouseState;

        #endregion

        private void PlatformSetCursor(HostCursor cursor)
        {
            throw new NotImplementedException();
            //Cursor = cursor?.Cursor;
        }
    }
}
