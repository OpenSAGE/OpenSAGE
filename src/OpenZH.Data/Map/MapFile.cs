using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using OpenZH.Data.RefPack;

namespace OpenZH.Data.Map
{
    public sealed class MapFile
    {
        public static MapFile Parse(BinaryReader reader)
        {
            var flag = reader.ReadUInt32();

            switch (flag)
            {
                case 1884121923u:
                    // Uncompressed
                    return ParseMapData(reader);

                case 5390661u:
                    // Compressed (after decompression, contents are exactly the same
                    // as uncompressed format, so we call back into this method)
                    var decompressedSize = reader.ReadUInt32();
                    var innerReader = new BinaryReader(new RefPackStream(reader.BaseStream));
                    return Parse(innerReader);

                default:
                    throw new NotSupportedException();
            }
        }

        private static MapFile ParseMapData(BinaryReader reader)
        {
            var assetStringsLength = reader.ReadUInt32();
            var assetStrings = new string[assetStringsLength];

            for (var i = (int) (assetStringsLength - 1); i >= 0; i--)
            {
                assetStrings[i] = reader.ReadString();
                var num = reader.ReadUInt32(); // Asset index?
                if (num != i + 1)
                {
                    throw new InvalidDataException();
                }
            }

            while (true) // TODO
            {
                var num1 = reader.ReadUInt32(); // Asset index?
                var num2 = reader.ReadUInt16();
                var num3 = reader.ReadUInt32(); // Length?

                var key = assetStrings[num1 - 1];

                switch (key)
                {
                    case "HeightMapData":
                        var heightMapData = HeightMapData.Parse(reader);
                        break;
                }
            }

            return new MapFile
            {

            };
        }
    }
}
