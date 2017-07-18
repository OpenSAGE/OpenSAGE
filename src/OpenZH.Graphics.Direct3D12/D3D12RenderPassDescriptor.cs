using System;
using SharpDX.Direct3D12;
using SharpDX.Mathematics.Interop;

namespace OpenZH.Graphics.Direct3D12
{
    public sealed class D3D12RenderPassDescriptor : RenderPassDescriptor
    {
        private class RenderTargetDescriptor
        {
            public D3D12RenderTargetView RenderTargetView;
            public LoadAction LoadAction;
            public RawColor4 ClearColor;
        }

        private RenderTargetDescriptor _renderTargetDescriptor;

        public override void SetRenderTargetDescriptor(RenderTargetView renderTargetView, LoadAction loadAction, ColorRgba clearColor)
        {
            _renderTargetDescriptor = new RenderTargetDescriptor
            {
                RenderTargetView = (D3D12RenderTargetView) renderTargetView,
                LoadAction = loadAction,
                ClearColor = clearColor.ToRawColor4()
            };
        }

        internal void OnOpenedCommandList(GraphicsCommandList commandList)
        {
            commandList.ResourceBarrierTransition(
                _renderTargetDescriptor.RenderTargetView.RenderTarget,
                ResourceStates.Present,
                ResourceStates.RenderTarget);

            commandList.SetRenderTargets(
                _renderTargetDescriptor.RenderTargetView.CpuDescriptorHandle,
                null);

            switch (_renderTargetDescriptor.LoadAction)
            {
                case LoadAction.DontCare:
                    break;

                case LoadAction.Load:
                    throw new NotSupportedException();

                case LoadAction.Clear:
                    commandList.ClearRenderTargetView(
                        _renderTargetDescriptor.RenderTargetView.CpuDescriptorHandle,
                        _renderTargetDescriptor.ClearColor);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        internal void OnClosingCommandList(GraphicsCommandList commandList)
        {
            commandList.ResourceBarrierTransition(
                _renderTargetDescriptor.RenderTargetView.RenderTarget,
                ResourceStates.RenderTarget,
                ResourceStates.Present);
        }
    }
}
