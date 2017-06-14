using System.IO;
using System.Runtime.InteropServices;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.W3d
{
    [StructLayout(LayoutKind.Sequential)]
    public struct W3dVector
    {
        public float X;
        public float Y;
        public float Z;

        public static W3dVector Parse(BinaryReader reader)
        {
            var result = new W3dVector();

            result.X = reader.ReadSingle();
            result.Y = reader.ReadSingle();
            result.Z = reader.ReadSingle();

            return result;
        }
    }
}
