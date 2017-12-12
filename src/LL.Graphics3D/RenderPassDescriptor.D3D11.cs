using System;
using LL.Graphics3D.Util;
using SharpDX.Mathematics.Interop;
using SharpDX.Direct3D11;

namespace LL.Graphics3D
{
    partial class RenderPassDescriptor
    {
        private class RenderTargetDescriptor
        {
            public RenderTarget RenderTarget;
            public LoadAction LoadAction;
            public RawColor4 ClearColor;
        }

        private RenderTargetDescriptor _renderTargetDescriptor;
        private DepthStencilBuffer _depthStencilBuffer;

        private void PlatformConstruct() { }

        private void PlatformSetRenderTargetDescriptor(RenderTarget renderTargetView, LoadAction loadAction, ColorRgbaF clearColor)
        {
            _renderTargetDescriptor = new RenderTargetDescriptor
            {
                RenderTarget = renderTargetView,
                LoadAction = loadAction,
                ClearColor = clearColor.ToRawColor4()
            };
        }

        private void PlatformSetDepthStencilDescriptor(DepthStencilBuffer depthStencilBuffer)
        {
            _depthStencilBuffer = depthStencilBuffer;
        }

        internal void OnOpenedCommandList(DeviceContext context)
        {
            var depthStencilView = _depthStencilBuffer?.DeviceDepthStencilView;

            context.OutputMerger.SetRenderTargets(
                depthStencilView,
                _renderTargetDescriptor.RenderTarget.DeviceRenderTargetView);

            switch (_renderTargetDescriptor.LoadAction)
            {
                case LoadAction.DontCare:
                    break;

                case LoadAction.Load:
                    throw new NotSupportedException();

                case LoadAction.Clear:
                    context.ClearRenderTargetView(
                        _renderTargetDescriptor.RenderTarget.DeviceRenderTargetView,
                        _renderTargetDescriptor.ClearColor);
                    break;

                default:
                    throw new InvalidOperationException();
            }

            if (depthStencilView != null)
            {
                context.ClearDepthStencilView(
                    depthStencilView,
                    DepthStencilClearFlags.Depth,
                    _depthStencilBuffer.ClearValue,
                    0);
            }
        }

        internal void OnClosedCommandList(DeviceContext context)
        {
            context.OutputMerger.SetRenderTargets(
                (DepthStencilView) null, 
                (RenderTargetView) null);
        }
    }
}
