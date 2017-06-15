using System.IO;
using System.Runtime.InteropServices;

namespace OpenZH.Data.W3d
{
    [StructLayout(LayoutKind.Sequential)]
    public struct W3dQuaternion
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public static W3dQuaternion Parse(BinaryReader reader)
        {
            return new W3dQuaternion
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle(),
                W = reader.ReadSingle()
            };
        }
    }
}
