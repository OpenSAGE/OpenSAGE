using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dHLodArrayHeader : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_HLOD_SUB_OBJECT_ARRAY_HEADER;

        public uint ModelCount { get; private set; }

        /// <summary>
        /// If model is bigger than this, switch to higher LOD.
        /// </summary>
        public float MaxScreenSize { get; private set; }

        internal static W3dHLodArrayHeader Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                return new W3dHLodArrayHeader
                {
                    ModelCount = reader.ReadUInt32(),
                    MaxScreenSize = reader.ReadSingle()
                };
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(ModelCount);
            writer.Write(MaxScreenSize);
        }
    }
}
