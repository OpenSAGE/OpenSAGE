using System;
using System.Collections.Generic;
using OpenSage.Input;
using OpenSage.Mathematics;
using OpenSage.Utilities;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage
{
    public sealed class GameWindow : DisposableBase
    {
        public event EventHandler ClientSizeChanged;

        private readonly Sdl2Window _window;

        private bool _closing;
        private int _lastMouseX;
        private int _lastMouseY;

        private void RaiseClientSizeChanged()
        {
            ClientSizeChanged?.Invoke(this, EventArgs.Empty);
        }

        public Swapchain Swapchain { get; private set; }

        public Rectangle ClientBounds
        {
            get
            {
                var result = _window.Bounds;
                return new Rectangle(0, 0, result.Width, result.Height);
            }
        }

        public bool IsCursorVisible
        {
            get => _window.CursorVisible;
            set => _window.CursorVisible = value;
        }

        public float WindowScale { get; private set; }

        public InputSnapshot CurrentInputSnapshot { get; private set; }

        public Queue<InputMessage> MessageQueue { get; } = new Queue<InputMessage>();

        public bool Fullscreen
        {
            get => _window.WindowState == WindowState.BorderlessFullScreen;
            set => _window.WindowState = value ? WindowState.BorderlessFullScreen : WindowState.Normal;
        }

        public bool Maximized
        {
            get { return _window.WindowState == WindowState.Maximized; }
            set { _window.WindowState = value ? WindowState.Maximized : WindowState.Normal; }
        }

        public GameWindow(string title, int x, int y, int width, int height, bool fullscreen)
        {
            var windowState = fullscreen ? WindowState.BorderlessFullScreen : WindowState.Normal;

            // Get display scale for primary monitor.
            // TODO: Track moving window to a different display,
            // which may change the scale.
            WindowScale = Sdl2Interop.GetDisplayScale(0);

            var windowCreateInfo = new WindowCreateInfo(
                (int)(WindowScale * x),
                (int)(WindowScale * y),
                (int)(WindowScale * width),
                (int)(WindowScale * height),
                windowState,
                title);

            _window = VeldridStartup.CreateWindow(ref windowCreateInfo);

            _window.KeyDown += HandleKeyDown;
            _window.KeyUp += HandleKeyUp;

            _window.MouseDown += HandleMouseDown;
            _window.MouseUp += HandleMouseUp;
            _window.MouseMove += HandleMouseMove;
            _window.MouseWheel += HandleMouseWheel;

            _window.Resized += HandleResized;

            _window.Closing += HandleClosing;
        }

        public GraphicsDevice CreateGraphicsDevice(GraphicsDeviceOptions options, GraphicsBackend backend)
        {
            var graphicsDevice = VeldridStartup.CreateGraphicsDevice(_window, options, backend);
            Swapchain = graphicsDevice.MainSwapchain;
            return graphicsDevice;
        }

        private void HandleClosing()
        {
            _closing = true;
        }

        private void HandleResized()
        {
            Swapchain.Resize(
                (uint) _window.Bounds.Width,
                (uint) _window.Bounds.Height);

            RaiseClientSizeChanged();
        }

        private void HandleKeyDown(KeyEvent evt)
        {
            var message = InputMessage.CreateKeyDown(evt.Key, evt.Modifiers);
            MessageQueue.Enqueue(message);
        }

        private void HandleKeyUp(KeyEvent evt)
        {
            var message = InputMessage.CreateKeyUp(evt.Key, evt.Modifiers);
            MessageQueue.Enqueue(message);
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
            MessageQueue.Enqueue(message);
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
            MessageQueue.Enqueue(message);
        }

        private void HandleMouseMove(MouseMoveEventArgs args)
        {
            _lastMouseX = args.State.X;
            _lastMouseY = args.State.Y;

            var message = InputMessage.CreateMouseMove(new Point2D(args.State.X, args.State.Y));
            MessageQueue.Enqueue(message);
        }

        private void HandleMouseWheel(MouseWheelEventArgs args)
        {
            if (args.WheelDelta == 0) {
                return;
            }

            var point = new Point2D(args.State.X, args.State.Y);
            var message = InputMessage.CreateMouseWheel((int) (args.WheelDelta * 100), point);
            MessageQueue.Enqueue(message);
        }

        public void Close()
        {
            _closing = true;
        }

        public bool PumpEvents()
        {
            CurrentInputSnapshot = _window.PumpEvents();

            if (_closing)
            {
                return false;
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
