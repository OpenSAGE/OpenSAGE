using System.IO;
using System.Numerics;
using System.Collections.Generic;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dNode : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_NODE;

        public string RenderObjectName { get; private set; }

        public uint PivotID { get; private set; }

        internal static W3dNode Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dNode
                {
                    RenderObjectName = reader.ReadFixedLengthString(W3dConstants.NameLength),
                    PivotID = reader.ReadUInt16()
                };

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(RenderObjectName);
            writer.Write(PivotID);
        }
    }
}
