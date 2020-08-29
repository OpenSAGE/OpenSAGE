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

        public GraphicsDevice GraphicsDevice { get; }

        public Rectangle ClientBounds
        {
            get
            {
                var result = _window.Bounds;
                return new Rectangle(0, 0, result.Width, result.Height);
            }
        }

        public bool IsMouseVisible
        {
            get => _window.CursorVisible;
            set => _window.CursorVisible = value;
        }

        public float WindowScale { get; private set; }

        public InputSnapshot CurrentInputSnapshot { get; private set; }

        public Queue<InputMessage> MessageQueue { get; } = new Queue<InputMessage>();

        public bool Fullscreen
        {
            get { return _window.WindowState == WindowState.BorderlessFullScreen; }
            set { _window.WindowState = value ? WindowState.BorderlessFullScreen : WindowState.Normal; }
        }

        public bool Maximized
        {
            get { return _window.WindowState == WindowState.Maximized; }
            set { _window.WindowState = value ? WindowState.Maximized : WindowState.Normal; }
        }

        internal GameWindow(string title, int x, int y, int width, int height,
                            GraphicsBackend? preferredBackend, bool fullscreen)
        {
#if DEBUG
            const bool debug = true;
#else
            const bool debug = false;
#endif

            var graphicsDeviceOptions = new GraphicsDeviceOptions(debug, null, true, ResourceBindingModel.Improved)
            {
                SwapchainSrgbFormat = false,
                PreferStandardClipSpaceYDirection = true,
                PreferDepthRangeZeroToOne = true
            };

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

            var backend = preferredBackend ?? VeldridStartup.GetPlatformDefaultBackend();

            VeldridStartup.CreateWindowAndGraphicsDevice(
                windowCreateInfo,
                graphicsDeviceOptions,
                backend,
                out _window,
                out var graphicsDevice);

            GraphicsDevice = AddDisposable(graphicsDevice);

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
            GraphicsDevice.ResizeMainWindow(
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
            var message = InputMessage.CreateMouseWheel((int) (args.WheelDelta * 100));
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

            GraphicsDevice.WaitForIdle();

            base.Dispose(disposeManagedResources);
        }
    }
}
