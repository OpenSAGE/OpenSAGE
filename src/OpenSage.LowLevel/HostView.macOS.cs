using System;
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

        public override void KeyDown(NSEvent theEvent)
        {
            throw new NotImplementedException();
        }

        public override void KeyUp(NSEvent theEvent)
        {
            throw new NotImplementedException();
        }

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
            RaiseMouseEvent(InputMessageType.MouseDown, MouseButton.Left, theEvent);
        }

        public override void MouseUp(NSEvent theEvent)
        {
            RaiseMouseEvent(InputMessageType.MouseUp, MouseButton.Left, theEvent);
        }

        public override void RightMouseDown(NSEvent theEvent)
        {
            RaiseMouseEvent(InputMessageType.MouseDown, MouseButton.Right, theEvent);
        }

        public override void RightMouseUp(NSEvent theEvent)
        {
            RaiseMouseEvent(InputMessageType.MouseUp, MouseButton.Right, theEvent);
        }

        public override void OtherMouseDown(NSEvent theEvent)
        {
            // TODO: theEvent.ButtonNumber
            RaiseMouseEvent(InputMessageType.MouseDown, MouseButton.Middle, theEvent);
        }

        public override void OtherMouseUp(NSEvent theEvent)
        {
            // TODO: theEvent.ButtonNumber
            RaiseMouseEvent(InputMessageType.MouseUp, MouseButton.Middle, theEvent);
        }

        public override void MouseMoved(NSEvent theEvent)
        {
            RaiseMouseEvent(InputMessageType.MouseMove, null, theEvent);
        }

        public override void MouseDragged(NSEvent theEvent)
        {
            RaiseMouseEvent(InputMessageType.MouseMove, MouseButton.Left, theEvent);
        }

        public override void RightMouseDragged(NSEvent theEvent)
        {
            RaiseMouseEvent(InputMessageType.MouseMove, MouseButton.Right, theEvent);
        }

        public override void OtherMouseDragged(NSEvent theEvent)
        {
            // TODO: theEvent.ButtonNumber
            RaiseMouseEvent(InputMessageType.MouseMove, MouseButton.Middle, theEvent);
        }

        private void RaiseMouseEvent(
            InputMessageType mouseMessageType,
            MouseButton? button,
            NSEvent theEvent)
        {
            var controlPosition = ConvertPointFromView(theEvent.LocationInWindow, null);

            RaiseInputMessage(new InputMessage(
                mouseMessageType,
                button,
                (int) controlPosition.X,
                (int) controlPosition.Y,
                (int) theEvent.ScrollingDeltaY));
        }

        public override void ScrollWheel(NSEvent theEvent)
        {
            RaiseMouseEvent(InputMessageType.MouseWheel, null, theEvent);
        }

        #endregion

        private void PlatformSetCursor(HostCursor cursor)
        {
            throw new NotImplementedException();
            //Cursor = cursor?.Cursor;
        }
    }
}
