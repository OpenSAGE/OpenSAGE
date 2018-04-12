using System;
using System.Collections.Generic;
using OpenSage.Input;
using OpenSage.Mathematics;
using Veldrid;
using Veldrid.Sdl2;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage
{
    public sealed class GameWindow : DisposableBase
    {
        public event EventHandler ClientSizeChanged;

        public Sdl2Window Sdl2Window { get; }

        private readonly Queue<InputMessage> _messageQueue = new Queue<InputMessage>();

        private bool _closing;
        private int _lastMouseX;
        private int _lastMouseY;

        private void RaiseClientSizeChanged()
        {
            ClientSizeChanged?.Invoke(this, EventArgs.Empty);
        }

        public Rectangle ClientBounds
        {
            get
            {
                var result = Sdl2Window.Bounds;
                return new Rectangle(0, 0, result.Width, result.Height);
            }
        }

        public event EventHandler<InputMessageEventArgs> InputMessageReceived;

        private void RaiseInputMessageReceived(InputMessageEventArgs args)
        {
            InputMessageReceived?.Invoke(this, args);
        }

        public bool IsMouseVisible
        {
            get => Sdl2Window.CursorVisible;
            set => Sdl2Window.CursorVisible = value;
        }

        // TODO: Remove this once we switch to Veldrid.
        public IntPtr NativeWindowHandle => Sdl2Window.Handle;

        public GameWindow(IntPtr windowsWindowHandle)
        {
            Sdl2Window = new Sdl2Window(windowsWindowHandle, false);
            AfterWindowCreated();
        }

        public GameWindow(string title, int x, int y, int width, int height)
        {
            Sdl2Window = new Sdl2Window(title, x, y, width, height, (SDL_WindowFlags) 0, false);
            AfterWindowCreated();
        }

        private void AfterWindowCreated()
        {
            Sdl2Window.KeyDown += HandleKeyDown;
            Sdl2Window.KeyUp += HandleKeyUp;

            Sdl2Window.MouseDown += HandleMouseDown;
            Sdl2Window.MouseUp += HandleMouseUp;
            Sdl2Window.MouseMove += HandleMouseMove;
            Sdl2Window.MouseWheel += HandleMouseWheel;

            Sdl2Window.Resized += HandleResized;

            Sdl2Window.Closing += HandleClosing;
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

        public void SetCursor(Cursor cursor)
        {
            // TODO
        }

        public bool PumpEvents()
        {
            // TODO: Use inputSnapshot instead of events?
            var inputSnapshot = Sdl2Window.PumpEvents();

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
            Sdl2Window.Close();

            base.Dispose(disposeManagedResources);
        }
    }
}
