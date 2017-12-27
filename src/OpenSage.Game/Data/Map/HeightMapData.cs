using System.Diagnostics;
using System.IO;

namespace OpenSage.Data.Map
{
    public sealed class HeightMapData : Asset
    {
        public const string AssetName = "HeightMapData";

        public uint Width { get; private set; }
        public uint Height { get; private set; }
        public uint BorderWidth { get; private set; }

        /// <summary>
        /// Relative to BorderWidth.
        /// </summary>
        public HeightMapPerimeter[] Perimeters { get; private set; }

        public uint Area { get; private set; }
        public ushort[,] Elevations { get; private set; }

        internal static HeightMapData Parse(BinaryReader reader, MapParseContext context)
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

                var elevations = new ushort[mapWidth, mapHeight];
                for (var y = 0; y < mapHeight; y++)
                {
                    for (var x = 0; x < mapWidth; x++)
                    {
                        elevations[x, y] = version >= 5
                            ? reader.ReadUInt16()
                            : reader.ReadByte();
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

        internal void WriteTo(BinaryWriter writer)
        {
            WriteAssetTo(writer, () =>
            {
                writer.Write(Width);
                writer.Write(Height);

                writer.Write(BorderWidth);

                writer.Write((uint) Perimeters.Length);

                foreach (var perimeter in Perimeters)
                {
                    writer.Write(perimeter.Width);
                    writer.Write(perimeter.Height);
                }

                writer.Write(Area);

                for (var y = 0; y < Height; y++)
                {
                    for (var x = 0; x < Width; x++)
                    {
                        if (Version >= 5)
                        {
                            writer.Write(Elevations[x, y]);
                        }
                        else
                        {
                            writer.Write((byte) Elevations[x, y]);
                        }
                    }
                }
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
