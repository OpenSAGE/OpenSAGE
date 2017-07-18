using System;
using Metal;

namespace OpenZH.Graphics.Metal
{
    public sealed class MetalCommandEncoder : CommandEncoder
    {
        private readonly IMTLRenderCommandEncoder _commandEncoder;

        public MetalCommandEncoder(IMTLRenderCommandEncoder commandEncoder)
        {
            _commandEncoder = commandEncoder;
        }

        public override void Close()
        {
            _commandEncoder.EndEncoding();
        }

        public override void DrawIndexed(
            PrimitiveType primitiveType,
            uint indexCount,
            IndexType indexType,
            Buffer indexBuffer,
            uint indexBufferOffset)
        {
            _commandEncoder.DrawIndexedPrimitives(
                primitiveType.ToMTLPrimitiveType(),
                indexCount,
                indexType.ToMTLIndexType(),
                ((MetalBuffer) indexBuffer).DeviceBuffer,
                indexBufferOffset);
        }

        public override void SetViewport(Viewport viewport)
        {
            _commandEncoder.SetViewport(viewport.ToMTLViewport());
            _commandEncoder.SetScissorRect(new MTLScissorRect(0, 0, (nuint) viewport.Width, (nuint) viewport.Height));
        }
    }
}