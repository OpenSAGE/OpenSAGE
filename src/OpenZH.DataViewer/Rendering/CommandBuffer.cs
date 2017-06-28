using System.Numerics;

namespace OpenZH.DataViewer.Rendering
{
    public abstract class CommandBuffer
    {
        public abstract void SetRenderTarget(RenderTargetView renderTargetView);

        public abstract void ClearRenderTarget(RenderTargetView renderTargetView, Vector4 colorRgba);

        public abstract void SetViewport(Viewport viewport);

        public abstract void Close();
    }
}
