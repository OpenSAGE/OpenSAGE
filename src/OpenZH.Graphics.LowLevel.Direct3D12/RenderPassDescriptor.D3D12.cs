using System;
using SharpDX.Direct3D12;
using SharpDX.Mathematics.Interop;
using OpenZH.Graphics.LowLevel.Util;

namespace OpenZH.Graphics.LowLevel
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

        private void PlatformConstruct() { }

        private void PlatformSetRenderTargetDescriptor(RenderTarget renderTargetView, LoadAction loadAction, ColorRgba clearColor)
        {
            _renderTargetDescriptor = new RenderTargetDescriptor
            {
                RenderTarget = renderTargetView,
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
