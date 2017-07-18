using System;
using Metal;

namespace OpenZH.Graphics.Metal
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