using System;
using CoreGraphics;
using MetalKit;
using UIKit;
using OpenZH.Graphics.Hosting;

namespace OpenZH.Graphics.Platforms.Apple
{
    public sealed class MetalGraphicsView : UIView, IMTKViewDelegate, IGraphicsView
    {
        public event EventHandler<GraphicsEventArgs> GraphicsInitialize;
        public event EventHandler<GraphicsEventArgs> GraphicsDraw;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly MTKView _metalView;
        private readonly SwapChain _swapChain;

        private bool _initialized;

        public bool RedrawsOnTimer
        {
            get { return !_metalView.Paused && !_metalView.EnableSetNeedsDisplay; }
            set
            {
                _metalView.Paused = !value;
                _metalView.EnableSetNeedsDisplay = !value;
            }
        }

        public MetalGraphicsView()
        {
            _graphicsDevice = new GraphicsDevice();

            _metalView = new MTKView(CGRect.Empty, _graphicsDevice.Device);
            _metalView.ColorPixelFormat = global::Metal.MTLPixelFormat.BGRA8Unorm;
            _metalView.Delegate = this;
            AddSubview(_metalView);

            _swapChain = new SwapChain(_graphicsDevice, _metalView);
        }

        public void Draw()
        {
            SetNeedsDisplay();
        }

        public override void WillMoveToSuperview(UIView newsuper)
        {
            if (!_initialized)
            {
                GraphicsInitialize?.Invoke(this, new GraphicsEventArgs(_graphicsDevice, _swapChain));
                _initialized = true;
            }

            base.WillMoveToSuperview(newsuper);
        }

        void IMTKViewDelegate.Draw(MTKView view)
        {
            GraphicsDraw?.Invoke(this, new GraphicsEventArgs(_graphicsDevice, _swapChain));
        }

        void IMTKViewDelegate.DrawableSizeWillChange(MTKView view, CGSize size)
        {
        }

        public override void LayoutSubviews()
        {
            _metalView.Frame = Bounds;

            base.LayoutSubviews();
        }
    }
}