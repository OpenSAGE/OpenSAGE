using System.Diagnostics;
using System.IO;

namespace OpenSage.Data.Map
{
    public sealed class HeightMapData : Asset
    {
        public const string AssetName = "HeightMapData";

        public bool ElevationsAre16Bit => Version >= 5;

        public uint Width { get; private set; }
        public uint Height { get; private set; }

        public uint BorderWidth { get; private set; }

        /// <summary>
        /// Relative to BorderWidth.
        /// </summary>
        public HeightMapPerimeter[] Perimeters { get; private set; }

        [AddedIn(SageGame.Cnc3)]
        public uint WidthExcludingBorder { get; private set; }

        [AddedIn(SageGame.Cnc3)]
        public uint HeightExcludingBorder { get; private set; }

        public uint Area { get; private set; }
        public ushort[,] Elevations { get; private set; }

        internal static HeightMapData Parse(BinaryReader reader, MapParseContext context)
        {
            return ParseAsset(reader, context, version =>
            {
                var result = new HeightMapData
                {
                    Width = reader.ReadUInt32(),
                    Height = reader.ReadUInt32(),

                    BorderWidth = reader.ReadUInt32()
                };

                var perimeterCount = reader.ReadUInt32();
                result.Perimeters = new HeightMapPerimeter[perimeterCount];

                for (var i = 0; i < perimeterCount; i++)
                {
                    result.Perimeters[i] = new HeightMapPerimeter
                    {
                        Width = reader.ReadUInt32(),
                        Height = reader.ReadUInt32()
                    };
                }

                if (version >= 6)
                {
                    result.WidthExcludingBorder = reader.ReadUInt32();
                    result.HeightExcludingBorder = reader.ReadUInt32();
                }

                result.Area = reader.ReadUInt32();
                if (result.Width * result.Height != result.Area)
                {
                    throw new InvalidDataException();
                }

                result.Elevations = new ushort[result.Width, result.Height];
                for (var y = 0; y < result.Height; y++)
                {
                    for (var x = 0; x < result.Width; x++)
                    {
                        result.Elevations[x, y] = version >= 5
                            ? reader.ReadUInt16()
                            : reader.ReadByte();
                    }
                }

                return result;
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

                if (Version >= 6)
                {
                    writer.Write(WidthExcludingBorder);
                    writer.Write(HeightExcludingBorder);
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
