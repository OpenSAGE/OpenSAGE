using System.IO;
using System.Runtime.InteropServices;

namespace OpenSage.Data.W3d
{
    [StructLayout(LayoutKind.Sequential)]
    public struct W3dRgbF
    {
        public float R { get; private set; }
        public float G { get; private set; }
        public float B { get; private set; }

        public static W3dRgbF Parse(BinaryReader reader)
        {
            var result = new W3dRgbF
            {
                R = reader.ReadSingle(),
                G = reader.ReadSingle(),
                B = reader.ReadSingle(),
            };

            return result;
        }

        public static void Write(BinaryWriter writer, W3dRgbF rgb)
        {
            writer.Write(rgb.R);
            writer.Write(rgb.G);
            writer.Write(rgb.B);
        }
    }
}
