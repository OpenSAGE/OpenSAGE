using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dBox : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_BOX;

        public uint Version { get; private set; }

        public W3dBoxType BoxType { get; private set; }

        public W3dBoxCollisionTypes CollisionTypes { get; private set; }

        public string Name { get; private set; }

        public W3dRgb Color { get; private set; }

        public Vector3 Center { get; private set; }

        public Vector3 Extent { get; private set; }

        internal static W3dBox Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dBox
                {
                    Version = reader.ReadUInt32()
                };

                var flags = reader.ReadUInt32();

                result.BoxType = (W3dBoxType) (flags & 0b11);
                result.CollisionTypes = (W3dBoxCollisionTypes) (flags & 0xFF0);

                result.Name = reader.ReadFixedLengthString(W3dConstants.NameLength * 2);
                result.Color = W3dRgb.Parse(reader);
                result.Center = reader.ReadVector3();
                result.Extent = reader.ReadVector3();

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(Version);

            var flags = (uint) BoxType | (uint) CollisionTypes;
            writer.Write(flags);

            writer.WriteFixedLengthString(Name, W3dConstants.NameLength * 2);
            writer.Write(Color);
            writer.Write(Center);
            writer.Write(Extent);
        }
    }
}
