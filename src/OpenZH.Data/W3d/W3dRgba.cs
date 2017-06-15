using System.IO;
using System.Runtime.InteropServices;

namespace OpenZH.Data.W3d
{
    [StructLayout(LayoutKind.Sequential)]
    public struct W3dRgba
    {
        public const int SizeInBytes = sizeof(byte) * 4;

        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public static W3dRgba Parse(BinaryReader reader)
        {
            return new W3dRgba
            {
                R = reader.ReadByte(),
                G = reader.ReadByte(),
                B = reader.ReadByte(),
                A = reader.ReadByte()
            };
        }
    }
}
