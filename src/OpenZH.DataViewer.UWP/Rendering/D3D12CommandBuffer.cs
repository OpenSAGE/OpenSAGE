using System;
using System.Numerics;
using OpenZH.DataViewer.Rendering;
using SharpDX.Direct3D12;
using SharpDX.Mathematics.Interop;

namespace OpenZH.DataViewer.UWP.Rendering
{
    public class D3D12CommandBuffer : CommandBuffer
    {
        private readonly GraphicsCommandList _commandList;

        public override void ClearRenderTarget(RenderTargetView renderTargetView, Vector4 colorRgba)
        {
            var d3d12View = (D3D12RenderTargetView) renderTargetView;
            _commandList.ClearRenderTargetView(d3d12View.CpuDescriptorHandle, colorRgba.ToRawColor4());
        }

        public override void SetRenderTarget(RenderTargetView renderTargetView)
        {
            var d3d12View = (D3D12RenderTargetView) renderTargetView;
            _commandList.SetRenderTargets(d3d12View.CpuDescriptorHandle, null);
        }

        public override void SetViewport(DataViewer.Rendering.Viewport viewport)
        {
            _commandList.SetViewport(viewport.ToViewportF());
            _commandList.SetScissorRectangles(new RawRectangle(0, 0, viewport.Width, viewport.Height));
        }

        public override void Close()
        {
            _commandList.Close();
        }
    }
}
