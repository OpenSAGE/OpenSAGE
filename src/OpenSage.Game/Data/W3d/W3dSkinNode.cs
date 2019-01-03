using System.IO;
using System.Numerics;
using System.Collections.Generic;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dSkinNode : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_SKIN_NODE;

        public byte[] UnknownBytes { get; private set; }

        internal static W3dSkinNode Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dSkinNode
                {
                    UnknownBytes = reader.ReadBytes((int) context.CurrentEndPosition - (int) reader.BaseStream.Position)
                };

                // TODO: Determine W3dSkinNode UnknownBytes

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(UnknownBytes);
        }
    }
}
