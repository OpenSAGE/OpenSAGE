using System.Windows;
using Caliburn.Micro;
using LLGfx;
using LLGfx.Hosting;
using OpenSage.DataViewer.ViewModels;

namespace OpenSage.DataViewer.Framework.Controls
{
    public sealed class RenderableControl : GraphicsDeviceControl
    {
        private DepthStencilBuffer _depthStencilBuffer;

        private bool _isLButtonDown;
        private bool _isMButtonDown;
        private bool _isRButtonDown;

        private Point _previousMousePoint;

        private IRenderableViewModel TypedDataContext => (IRenderableViewModel) DataContext;

        public RenderableControl()
        {
            GraphicsDevice = IoC.Get<GraphicsDeviceManager>().GraphicsDevice;

            RedrawsOnTimer = true;
        }

        protected override void OnUnloaded(object sender, RoutedEventArgs e)
        {
            base.OnUnloaded(sender, e);

            TypedDataContext.Dispose();

            if (_depthStencilBuffer != null)
            {
                _depthStencilBuffer.Dispose();
                _depthStencilBuffer = null;
            }
        }

        protected override void Draw()
        {
            var renderPassDescriptor = new RenderPassDescriptor();

            renderPassDescriptor.SetRenderTargetDescriptor(
                SwapChain.GetNextRenderTarget(),
                LoadAction.Clear,
                new ColorRgba(0.6f, 0.6f, 0.6f, 1));

            renderPassDescriptor.SetDepthStencilDescriptor(_depthStencilBuffer);

            TypedDataContext.Draw(GraphicsDevice, SwapChain, renderPassDescriptor);
        }

        protected override void OnSwapChainResized(SwapChain newSwapChain)
        {
            if (_depthStencilBuffer != null
                && _depthStencilBuffer.Width == newSwapChain.BackBufferWidth
                && _depthStencilBuffer.Height == newSwapChain.BackBufferHeight)
            {
                return;
            }

            if (_depthStencilBuffer != null)
            {
                _depthStencilBuffer.Dispose();
                _depthStencilBuffer = null;
            }

            _depthStencilBuffer = new DepthStencilBuffer(
                GraphicsDevice,
                newSwapChain.BackBufferWidth,
                newSwapChain.BackBufferHeight,
                1.0f);
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
