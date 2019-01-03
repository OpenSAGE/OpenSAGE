using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dTextureInfo : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_TEXTURE_INFO;

        /// <summary>
        /// Flags for this texture
        /// </summary>
        public W3dTextureFlags Attributes { get; private set; }

        /// <summary>
        /// animation logic
        /// </summary>
        public W3dTextureAnimation AnimationType { get; private set; }

        /// <summary>
        /// Number of frames (1 if not animated)
        /// </summary>
        public uint FrameCount { get; private set; }

        /// <summary>
        /// Frame rate, frames per second in floating point
        /// </summary>
        public float FrameRate { get; private set; }

        internal static W3dTextureInfo Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                return new W3dTextureInfo
                {
                    Attributes = reader.ReadUInt16AsEnumFlags<W3dTextureFlags>(),
                    AnimationType = reader.ReadUInt16AsEnum<W3dTextureAnimation>(),
                    FrameCount = reader.ReadUInt32(),
                    FrameRate = reader.ReadSingle(),
                };
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write((ushort) Attributes);
            writer.Write((ushort) AnimationType);
            writer.Write(FrameCount);
            writer.Write(FrameRate);
        }
    }
}
