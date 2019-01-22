using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    public sealed class CliffTextureMapping
    {
        public uint TextureTile { get; private set; }

        public Vector2 BottomLeftCoords { get; private set; }
        public Vector2 BottomRightCoords { get; private set; }
        public Vector2 TopRightCoords { get; private set; }
        public Vector2 TopLeftCoords { get; private set; }

        public ushort Unknown2 { get; private set; }

        internal static CliffTextureMapping Parse(BinaryReader reader)
        {
            return new CliffTextureMapping
            {
                TextureTile = reader.ReadUInt32(),

                BottomLeftCoords = reader.ReadVector2(),
                BottomRightCoords = reader.ReadVector2(),
                TopRightCoords = reader.ReadVector2(),
                TopLeftCoords = reader.ReadVector2(),

                Unknown2 = reader.ReadUInt16()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(TextureTile);

            writer.Write(BottomLeftCoords);
            writer.Write(BottomRightCoords);
            writer.Write(TopRightCoords);
            writer.Write(TopLeftCoords);

            writer.Write(Unknown2);
        }
    }
}
