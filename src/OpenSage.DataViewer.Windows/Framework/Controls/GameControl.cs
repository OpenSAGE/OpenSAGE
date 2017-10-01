using System.Windows;
using Caliburn.Micro;
using LLGfx.Hosting;
using OpenSage.DataViewer.ViewModels;

namespace OpenSage.DataViewer.Framework.Controls
{
    public sealed class GameControl : GraphicsDeviceControl
    {
        private bool _isLButtonDown;
        private bool _isMButtonDown;
        private bool _isRButtonDown;

        private Point _previousMousePoint;

        private IGameViewModel TypedDataContext => (IGameViewModel) DataContext;

        public GameControl()
        {
            GraphicsDevice = IoC.Get<GraphicsDeviceManager>().GraphicsDevice;

            RedrawsOnTimer = true;
        }

        protected override void OnUnloaded(object sender, RoutedEventArgs e)
        {
            base.OnUnloaded(sender, e);

            TypedDataContext.Dispose();
        }

        protected override void RaiseGraphicsInitialize(GraphicsEventArgs args)
        {
            base.RaiseGraphicsInitialize(args);

            TypedDataContext.Game.Initialize(SwapChain);
        }

        protected override void Draw()
        {
            TypedDataContext.Game.Tick();
        }

        protected override void RaiseHwndLButtonDown(HwndMouseEventArgs args)
        {
            base.RaiseHwndLButtonDown(args);

            _isLButtonDown = true;

            _previousMousePoint = args.ScreenPosition;
        }

        protected override void RaiseHwndLButtonUp(HwndMouseEventArgs args)
        {
            base.RaiseHwndLButtonUp(args);

            ReleaseMouseCapture();
            _isLButtonDown = false;
        }

        protected override void RaiseHwndMButtonDown(HwndMouseEventArgs args)
        {
            base.RaiseHwndMButtonDown(args);

            CaptureMouse();
            _isMButtonDown = true;

            _previousMousePoint = args.ScreenPosition;
        }

        protected override void RaiseHwndMButtonUp(HwndMouseEventArgs args)
        {
            base.RaiseHwndMButtonUp(args);

            ReleaseMouseCapture();
            _isMButtonDown = false;
        }

        protected override void RaiseHwndRButtonDown(HwndMouseEventArgs args)
        {
            base.RaiseHwndRButtonDown(args);

            CaptureMouse();
            _isRButtonDown = true;

            _previousMousePoint = args.ScreenPosition;
        }

        protected override void RaiseHwndRButtonUp(HwndMouseEventArgs args)
        {
            base.RaiseHwndRButtonUp(args);

            ReleaseMouseCapture();
            _isRButtonDown = false;
        }

        protected override void RaiseHwndMouseMove(HwndMouseEventArgs args)
        {
            base.RaiseHwndMouseMove(args);

            var delta = _previousMousePoint - args.ScreenPosition;

            if (_isLButtonDown)
            {
                TypedDataContext.CameraController.OnLeftMouseButtonDragged((float) delta.X, (float) delta.Y);
            }
            else if (_isMButtonDown)
            {
                TypedDataContext.CameraController.OnMiddleMouseButtonDragged((float) delta.Y);
            }
            else if (_isRButtonDown)
            {
                TypedDataContext.CameraController.OnRightMouseButtonDragged((float) delta.X, (float) delta.Y);
            }

            var mousePosition = args.GetPosition(this);
            TypedDataContext.OnMouseMove(
                (int) mousePosition.X,
                (int) mousePosition.Y);

            _previousMousePoint = args.ScreenPosition;
        }
    }
}
