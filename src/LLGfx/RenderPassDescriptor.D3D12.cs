using System;
using SharpDX.Direct3D12;
using SharpDX.Mathematics.Interop;
using LLGfx.Util;

namespace LLGfx
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

        private void PlatformSetRenderTargetDescriptor(RenderTarget renderTargetView, LoadAction loadAction, ColorRgba clearColor)
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

        internal void OnOpenedCommandList(GraphicsCommandList commandList)
        {
            commandList.ResourceBarrierTransition(
                _renderTargetDescriptor.RenderTarget.Texture,
                ResourceStates.Present,
                ResourceStates.RenderTarget);

            var depthStencilBufferCpuDescriptorHandle = _depthStencilBuffer?.Acquire();

            commandList.SetRenderTargets(
                _renderTargetDescriptor.RenderTarget.CpuDescriptorHandle,
                depthStencilBufferCpuDescriptorHandle);

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

            if (depthStencilBufferCpuDescriptorHandle != null)
            {
                commandList.ClearDepthStencilView(
                    depthStencilBufferCpuDescriptorHandle.Value,
                    ClearFlags.FlagsDepth,
                    _depthStencilBuffer.ClearValue,
                    0);
            }
        }

        internal void OnClosingCommandList(GraphicsCommandList commandList)
        {
            _depthStencilBuffer?.Release();

            commandList.ResourceBarrierTransition(
                _renderTargetDescriptor.RenderTarget.Texture,
                ResourceStates.RenderTarget,
                ResourceStates.Present);
        }
    }
}
