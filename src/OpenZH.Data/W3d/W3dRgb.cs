using System.IO;
using System.Runtime.InteropServices;

namespace OpenZH.Data.W3d
{
    [StructLayout(LayoutKind.Sequential)]
    public struct W3dRgb
    {
        public byte R;
        public byte G;
        public byte B;

        public static W3dRgb Parse(BinaryReader reader)
        {
            var result = new W3dRgb
            {
                R = reader.ReadByte(),
                G = reader.ReadByte(),
                B = reader.ReadByte(),
            };

            reader.ReadByte(); // padding

            return result;
        }
    }
}
