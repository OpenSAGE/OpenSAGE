using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Graphics.Util
{
    public static class ConversionExtensions
    {
        public static Vector3 ToVector3(this IniColorRgb value)
        {
            return new Vector3(value.R / 255.0f, value.G / 255.0f, value.B / 255.0f);
        }

        public static Vector3 ToVector3(this Coord3D value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }
    }
}
