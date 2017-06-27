using OpenZH.Data.W3d;
using SharpDX.Mathematics.Interop;

namespace OpenZH.DataViewer.UWP.Renderers
{
    internal static class ConversionExtensions
    {
        public static RawVector3 ToRawVector3(this W3dRgb v)
        {
            return new RawVector3(v.R / 255.0f, v.G / 255.0f, v.B / 255.0f);
        }
    }
}
