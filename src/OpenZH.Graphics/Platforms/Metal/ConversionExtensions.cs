using System;
using Metal;

namespace OpenZH.Graphics.Platforms.Metal
{
    internal static class ConversionExtensions
    {
        public static MTLClearColor ToMTLClearColor(this ColorRgba value)
        {
            return new MTLClearColor(value.R, value.G, value.B, value.A);
        }

        public static MTLIndexType ToMTLIndexType(this IndexType value)
        {
            switch (value)
            {
                case IndexType.UInt16:
                    return MTLIndexType.UInt16;

                case IndexType.UInt32:
                    return MTLIndexType.UInt32;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static MTLLoadAction ToMTLLoadAction(this LoadAction value)
        {
            switch (value)
            {
                case LoadAction.DontCare:
                    return MTLLoadAction.DontCare;

                case LoadAction.Clear:
                    return MTLLoadAction.Clear;

                case LoadAction.Load:
                    return MTLLoadAction.Load;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static MTLPixelFormat ToMTLPixelFormat(this PixelFormat value)
        {
            switch (value)
            {
                case PixelFormat.Bc1:
                case PixelFormat.Bc2:
                case PixelFormat.Bc3:
                    // We will decompress these formats manually in MetalTexture.
                    return MTLPixelFormat.RGBA8Unorm;

                case PixelFormat.Rgba8UNorm:
                    return MTLPixelFormat.RGBA8Unorm;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static MTLPrimitiveType ToMTLPrimitiveType(this PrimitiveType value)
        {
            switch (value)
            {
                case PrimitiveType.TriangleList:
                    return MTLPrimitiveType.Triangle;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static MTLVertexFormat ToMTLVertexFormat(this VertexFormat value)
        {
            switch (value)
            {
                case VertexFormat.Float2:
                    return MTLVertexFormat.Float4;

                case VertexFormat.Float3:
                    return MTLVertexFormat.Float3;

                case VertexFormat.Float4:
                    return MTLVertexFormat.Float4;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static MTLViewport ToMTLViewport(this Viewport value)
        {
            return new MTLViewport(
                value.X,
                value.Y,
                value.Width,
                value.Height,
                value.MinDepth,
                value.MaxDepth);
        }
    }
}