namespace OpenZH.Graphics
{
    public abstract class SwapChain : GraphicsObject
    {
        public abstract RenderTarget GetNextRenderTarget();
    }
}
