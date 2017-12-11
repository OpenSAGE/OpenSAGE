using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSage.DataViewer.Framework
{
    internal static class BmpUtility
    {
        public static byte[] PrependBmpHeader(byte[] rgbaData, int width, int height)
        {
            using (var stream = new MemoryStream(54 + rgbaData.Length))
            using (var binaryWriter = new BinaryWriter(stream))
            {
                binaryWriter.Write((ushort) 0x4D42);
                binaryWriter.Write((uint) stream.Length);
                binaryWriter.Write((ushort) 0);
                binaryWriter.Write((ushort) 0);
                binaryWriter.Write((uint) 54);
                binaryWriter.Write((uint) 40);
                binaryWriter.Write((uint) width);
                binaryWriter.Write((uint) height);
                binaryWriter.Write((ushort) 1);
                binaryWriter.Write((ushort) 32);
                binaryWriter.Write((uint) 0);
                binaryWriter.Write((uint) rgbaData.Length);
                binaryWriter.Write((uint) 96);
                binaryWriter.Write((uint) 96);
                binaryWriter.Write((uint) 0);
                binaryWriter.Write((uint) 0);

                binaryWriter.Write(rgbaData);

                return stream.ToArray();
            }
        }
    }
}
