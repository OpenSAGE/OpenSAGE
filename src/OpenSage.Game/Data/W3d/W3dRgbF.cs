using OpenSage.Mathematics;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenSage.Data.W3d
{
    public static class W3dRgbF
    {
        public static ColorRgbF Parse(BinaryReader reader)
        {
            var result = new ColorRgbF ( 
                reader.ReadSingle(),
                reader.ReadSingle(),
                reader.ReadSingle()
            );

            return result;
        }

        public static void Write(BinaryWriter writer, ColorRgbF rgbf)
        {
            writer.Write(rgbf.R);
            writer.Write(rgbf.G);
            writer.Write(rgbf.B);
        }
    }
}
