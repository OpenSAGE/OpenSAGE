using System.IO;
using System.Numerics;
using System.Collections.Generic;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dShadowNode : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.OBSOLETE_W3D_CHUNK_SHADOW_NODE;

        public byte[] UnknownBytes { get; private set; }

        internal static W3dShadowNode Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dShadowNode
                {
                    UnknownBytes = reader.ReadBytes((int)context.CurrentEndPosition - (int)reader.BaseStream.Position)
                };

                // TODO: Determine W3dShadowNode UnknownBytes

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(UnknownBytes);
        }
    }
}
