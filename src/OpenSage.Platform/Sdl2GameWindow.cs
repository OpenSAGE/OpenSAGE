using System;
using System.Collections.Generic;
using OpenSage.Input;
using OpenSage.Mathematics;
using Veldrid;
using Veldrid.Sdl2;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage
{
    public unsafe sealed class Sdl2GameWindow : GameWindow
    {
        private readonly Sdl2Window _window;

        private readonly Queue<InputMessage> _messageQueue = new Queue<InputMessage>();

        private bool _closing;
        private int _lastMouseX;
        private int _lastMouseY;

        public override Rectangle ClientBounds
        {
            get
            {
                var result = _window.Bounds;
                return new Rectangle(0, 0, result.Width, result.Height);
            }
        }

        public override bool IsMouseVisible
        {
            get => Sdl2Native.SDL_ShowCursor(Sdl2Native.SDL_QUERY) == 1;
            set => Sdl2Native.SDL_ShowCursor(value ? Sdl2Native.SDL_ENABLE : Sdl2Native.SDL_DISABLE);
        }

        public override IntPtr NativeWindowHandle => _window.Handle;

        public Sdl2GameWindow(IntPtr windowsWindowHandle)
        {
            _window = new Sdl2Window(windowsWindowHandle, false);
            AfterWindowCreated();
        }

        public Sdl2GameWindow(string title, int x, int y, int width, int height)
        {
            _window = new Sdl2Window(title, x, y, width, height, (SDL_WindowFlags) 0, false);
            AfterWindowCreated();
        }

        private void AfterWindowCreated()
        {
            _window.KeyDown += HandleKeyDown;
            _window.KeyUp += HandleKeyUp;

            _window.MouseDown += HandleMouseDown;
            _window.MouseUp += HandleMouseUp;
            _window.MouseMove += HandleMouseMove;
            _window.MouseWheel += HandleMouseWheel;

            _window.Resized += HandleResized;

            _window.Closing += HandleClosing;
        }

        private void HandleClosing()
        {
            _closing = true;
        }

        private void HandleResized()
        {
            RaiseClientSizeChanged();
        }

        private void HandleKeyDown(KeyEvent evt)
        {
            var message = InputMessage.CreateKeyDown(evt.Key);
            _messageQueue.Enqueue(message);
        }

        private void HandleKeyUp(KeyEvent evt)
        {
            var message = InputMessage.CreateKeyUp(evt.Key);
            _messageQueue.Enqueue(message);
        }

        private void HandleMouseDown(MouseEvent evt)
        {
            InputMessageType? getMessageType()
            {
                switch (evt.MouseButton)
                {
                    case MouseButton.Left:
                        return InputMessageType.MouseLeftButtonDown;
                    case MouseButton.Middle:
                        return InputMessageType.MouseMiddleButtonDown;
                    case MouseButton.Right:
                        return InputMessageType.MouseRightButtonDown;
                    default:
                        return null;
                }
            }

            var messageType = getMessageType();
            if (messageType == null)
            {
                return;
            }

            var message = InputMessage.CreateMouseButton(messageType.Value, new Point2D(_lastMouseX, _lastMouseY));
            _messageQueue.Enqueue(message);
        }

        private void HandleMouseUp(MouseEvent evt)
        {
            InputMessageType? getMessageType()
            {
                switch (evt.MouseButton)
                {
                    case MouseButton.Left:
                        return InputMessageType.MouseLeftButtonUp;
                    case MouseButton.Middle:
                        return InputMessageType.MouseMiddleButtonUp;
                    case MouseButton.Right:
                        return InputMessageType.MouseRightButtonUp;
                    default:
                        return null;
                }
            }

            var messageType = getMessageType();
            if (messageType == null)
            {
                return;
            }

            var message = InputMessage.CreateMouseButton(messageType.Value, new Point2D(_lastMouseX, _lastMouseY));
            _messageQueue.Enqueue(message);
        }

        private void HandleMouseMove(MouseMoveEventArgs args)
        {
            var message = InputMessage.CreateMouseMove(new Point2D(args.State.X, args.State.Y));
            _messageQueue.Enqueue(message);
        }

        private void HandleMouseWheel(MouseWheelEventArgs args)
        {
            var message = InputMessage.CreateMouseWheel((int) (args.WheelDelta * 100));
            _messageQueue.Enqueue(message);
        }

        public override void SetCursor(Cursor cursor)
        {
            // TODO
        }

        public override bool PumpEvents()
        {
            // TODO: Use inputSnapshot instead of events?
            var inputSnapshot = _window.PumpEvents();

            // TODO: This isn't right, it means button events might not have the right position.
            _lastMouseX = (int) inputSnapshot.MousePosition.X;
            _lastMouseY = (int) inputSnapshot.MousePosition.Y;

            if (_closing)
            {
                return false;
            }

            while (_messageQueue.Count > 0)
            {
                RaiseInputMessageReceived(new InputMessageEventArgs(_messageQueue.Dequeue()));
            }

            return true;
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            // TODO: This isn't right.
            _window.Close();

            base.Dispose(disposeManagedResources);
        }
    }
}
