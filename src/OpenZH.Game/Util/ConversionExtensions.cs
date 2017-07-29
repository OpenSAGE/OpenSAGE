using System.Numerics;
using OpenZH.Data.W3d;

namespace OpenZH.Game.Util
{
    public static class ConversionExtensions
    {
        public static Vector3 ToVector3(this W3dVector value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }

        public static Vector3 ToVector3(this W3dRgb value)
        {
            return new Vector3(value.R, value.G, value.B);
        }
    }
}
