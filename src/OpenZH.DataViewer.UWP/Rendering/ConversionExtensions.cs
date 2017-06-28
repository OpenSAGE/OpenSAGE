using System.Numerics;
using OpenZH.DataViewer.Rendering;
using SharpDX.Mathematics.Interop;

namespace OpenZH.DataViewer.UWP.Rendering
{
    internal static class ConversionExtensions
    {
        public static RawColor4 ToRawColor4(this Vector4 v)
        {
            return new RawColor4(v.X, v.Y, v.Z, v.W);
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
    }
}
