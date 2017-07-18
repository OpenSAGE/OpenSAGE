using System;
using CoreGraphics;
using MetalKit;
using OpenZH.Graphics.Metal;
using UIKit;

namespace OpenZH.Platform.iOS
{
    public sealed class MetalGraphicsView : UIView, IMTKViewDelegate, IGraphicsView
    {
        public event EventHandler<GraphicsEventArgs> GraphicsInitialize;
        public event EventHandler<GraphicsDrawEventArgs> GraphicsDraw;

        private readonly MetalGraphicsDevice _graphicsDevice;
        private readonly MTKView _metalView;
        private readonly MetalSwapChain _swapChain;
        private readonly MetalCommandQueue _commandQueue;

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
            var graphicsDevice = new MetalGraphicsDevice();

            _metalView = new MTKView(CGRect.Empty, graphicsDevice.Device);
            _metalView.Delegate = this;
            AddSubview(_metalView);

            _swapChain = new MetalSwapChain(_metalView);
        }

        public void Draw()
        {
            SetNeedsDisplay();
        }

        public override void WillMoveToSuperview(UIView newsuper)
        {
            if (!_initialized)
            {
                GraphicsInitialize?.Invoke(this, new GraphicsEventArgs(_graphicsDevice));
                _initialized = true;
            }

            base.WillMoveToSuperview(newsuper);
        }

        void IMTKViewDelegate.Draw(MTKView view)
        {
            GraphicsDraw?.Invoke(this, new GraphicsDrawEventArgs(_graphicsDevice, _swapChain));
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