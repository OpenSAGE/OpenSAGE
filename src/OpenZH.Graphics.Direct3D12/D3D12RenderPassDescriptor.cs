using System;
using SharpDX.Direct3D12;
using SharpDX.Mathematics.Interop;

namespace OpenZH.Graphics.Direct3D12
{
    public sealed class D3D12RenderPassDescriptor : RenderPassDescriptor
    {
        private class RenderTargetDescriptor
        {
            public D3D12RenderTarget RenderTarget;
            public LoadAction LoadAction;
            public RawColor4 ClearColor;
        }

        private RenderTargetDescriptor _renderTargetDescriptor;

        public override void SetRenderTargetDescriptor(RenderTarget renderTargetView, LoadAction loadAction, ColorRgba clearColor)
        {
            _renderTargetDescriptor = new RenderTargetDescriptor
            {
                RenderTarget = (D3D12RenderTarget) renderTargetView,
                LoadAction = loadAction,
                ClearColor = clearColor.ToRawColor4()
            };
        }

        internal void OnOpenedCommandList(GraphicsCommandList commandList)
        {
            commandList.ResourceBarrierTransition(
                _renderTargetDescriptor.RenderTarget.Texture,
                ResourceStates.Present,
                ResourceStates.RenderTarget);

            commandList.SetRenderTargets(
                _renderTargetDescriptor.RenderTarget.CpuDescriptorHandle,
                null);

            switch (_renderTargetDescriptor.LoadAction)
            {
                case LoadAction.DontCare:
                    break;

                case LoadAction.Load:
                    throw new NotSupportedException();

                case LoadAction.Clear:
                    commandList.ClearRenderTargetView(
                        _renderTargetDescriptor.RenderTarget.CpuDescriptorHandle,
                        _renderTargetDescriptor.ClearColor);
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        internal void OnClosingCommandList(GraphicsCommandList commandList)
        {
            commandList.ResourceBarrierTransition(
                _renderTargetDescriptor.RenderTarget.Texture,
                ResourceStates.RenderTarget,
                ResourceStates.Present);
        }
    }
}
