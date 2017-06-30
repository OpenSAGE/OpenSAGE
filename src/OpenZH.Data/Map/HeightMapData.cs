using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.Map
{
    public sealed class HeightMapData
    {
        public static HeightMapData Parse(BinaryReader reader)
        {
            var mapWidth = reader.ReadUInt32();
            var mapHeight = reader.ReadUInt32();

            if (mapWidth > 1000 || mapHeight > 1000)
            {
                throw new InvalidDataException("Map size too big?");
            }
            else if (mapWidth < 50 || mapHeight < 50)
            {
                throw new InvalidDataException("Map size too small?");
            }

            var borderWidth = reader.ReadUInt32();

            // Unknown block
            var unknown = reader.ReadUInt32();
            //var unknownBlock = reader.ReadBytes((int) ((unknownBlockSize - 1) * 16 + 8));

            var playableWidth = reader.ReadUInt32();
            var playableHeight = reader.ReadUInt32();

            var area = reader.ReadUInt32();
            if (mapWidth * mapHeight != area)
            {
                throw new InvalidDataException();
            }

            var elevations = new byte[mapWidth, mapHeight];
            for (var y = 0; y < mapHeight; y++)
            {
                for (var x = 0; x < mapWidth; x++)
                {
                    elevations[x, y] = reader.ReadByte();
                }
            }

            return new HeightMapData();
        }
    }
}
