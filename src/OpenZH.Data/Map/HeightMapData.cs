using System.Diagnostics;
using System.IO;

namespace OpenZH.Data.Map
{
    public sealed class HeightMapData : Asset
    {
        public uint Width { get; private set; }
        public uint Height { get; private set; }
        public uint BorderWidth { get; private set; }

        /// <summary>
        /// Relative to BorderWidth.
        /// </summary>
        public HeightMapPerimeter[] Perimeters { get; private set; }

        public uint Area { get; private set; }
        public byte[,] Elevations { get; private set; }

        public static HeightMapData Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var mapWidth = reader.ReadUInt32();
                var mapHeight = reader.ReadUInt32();

                var borderWidth = reader.ReadUInt32();

                var perimeterCount = reader.ReadUInt32();
                var perimeters = new HeightMapPerimeter[perimeterCount];

                for (var i = 0; i < perimeterCount; i++)
                {
                    perimeters[i] = new HeightMapPerimeter
                    {
                        Width = reader.ReadUInt32(),
                        Height = reader.ReadUInt32()
                    };
                }

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

                return new HeightMapData
                {
                    Width = mapWidth,
                    Height = mapHeight,
                    BorderWidth = borderWidth,
                    Perimeters = perimeters,
                    Area = area,
                    Elevations = elevations
                };
            });
        }
    }

    [DebuggerDisplay("Width = {Width}, Height = {Height}")]
    public struct HeightMapPerimeter
    {
        public uint Width;
        public uint Height;
    }
}
