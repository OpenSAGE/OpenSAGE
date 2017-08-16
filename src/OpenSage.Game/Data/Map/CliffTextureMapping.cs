using System.IO;

namespace OpenSage.Data.Map
{
    public sealed class CliffTextureMapping
    {
        public uint TextureTile { get; private set; }

        public MapTexCoord BottomLeftCoords { get; private set; }
        public MapTexCoord BottomRightCoords { get; private set; }
        public MapTexCoord TopRightCoords { get; private set; }
        public MapTexCoord TopLeftCoords { get; private set; }

        public ushort Unknown2 { get; private set; }

        internal static CliffTextureMapping Parse(BinaryReader reader)
        {
            return new CliffTextureMapping
            {
                TextureTile = reader.ReadUInt32(),

                BottomLeftCoords = MapTexCoord.Parse(reader),
                BottomRightCoords = MapTexCoord.Parse(reader),
                TopRightCoords = MapTexCoord.Parse(reader),
                TopLeftCoords = MapTexCoord.Parse(reader),

                Unknown2 = reader.ReadUInt16()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(TextureTile);

            BottomLeftCoords.WriteTo(writer);
            BottomRightCoords.WriteTo(writer);
            TopRightCoords.WriteTo(writer);
            TopLeftCoords.WriteTo(writer);

            writer.Write(Unknown2);
        }
    }
}
