using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Util
{
    public static class ConversionExtensions
    {
        public static Vector3 ToVector3(this in ColorRgb value)
        {
            return new Vector3(value.R / 255.0f, value.G / 255.0f, value.B / 255.0f);
        }
    }
}
