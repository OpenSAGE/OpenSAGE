using System;
using System.Numerics;
using SharpDX.Direct3D;
using SharpDX.Direct3D12;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace OpenZH.Graphics.Direct3D12
{
    internal static class ConversionExtensions
    {
        public static Format ToDxgiFormat(this IndexType value)
        {
            switch (value)
            {
                case IndexType.UInt16:
                    return Format.R16_UInt;

                case IndexType.UInt32:
                    return Format.R32_UInt;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static PrimitiveTopology ToPrimitiveTopology(this PrimitiveType value)
        {
            switch (value)
            {
                case PrimitiveType.TriangleList:
                    return PrimitiveTopology.TriangleList;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static RawColor4 ToRawColor4(this ColorRgba value)
        {
            return new RawColor4(value.R, value.G, value.B, value.A);
        }

        public static RawViewportF ToViewportF(this Viewport v)
        {
            return new RawViewportF
            {
                X = v.X,
                Y = v.Y,
                Width = v.Width,
                Height = v.Height,
                MinDepth = v.MinDepth,
                MaxDepth = v.MaxDepth
            };
        }

        public static ResourceStates ToResourceStates(this GraphicsResourceState resourceState)
        {
            switch (resourceState)
            {
                case GraphicsResourceState.Present:
                    return ResourceStates.Present;

                case GraphicsResourceState.RenderTarget:
                    return ResourceStates.RenderTarget;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
