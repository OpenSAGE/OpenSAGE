using Metal;
using MetalKit;

namespace OpenZH.Graphics.Metal
{
    public sealed class MetalSwapChain : SwapChain
    {
        private readonly MTKView _metalView;

        internal IMTLDrawable CurrentDrawable => _metalView.CurrentDrawable;

        public MetalSwapChain(MTKView metalView)
        {
            _metalView = metalView;
        }

        public override RenderTargetView GetNextRenderTarget()
        {
            return new MetalRenderTargetView(_metalView.CurrentDrawable.Texture);
        }
    }
}