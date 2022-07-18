using System.IO;
using System.Runtime.InteropServices;

namespace OpenSage.FileFormats.W3d
{
    [StructLayout(LayoutKind.Sequential)]
    public struct W3dVectorUInt32
    {
        public const int SizeInBytes = sizeof(uint) * 3;

        public uint X;
        public uint Y;
        public uint Z;

        public static W3dVectorUInt32 Parse(BinaryReader reader)
        {
            return new W3dVectorUInt32
            {
                X = reader.ReadUInt32(),
                Y = reader.ReadUInt32(),
                Z = reader.ReadUInt32()
            };
        }
    }
}
