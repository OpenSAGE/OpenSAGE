using System.IO;
using System.Numerics;
using System.Collections.Generic;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dDazzleTypeName : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_DAZZLE_TYPENAME;

        public string TypeName { get; private set; }

        internal static W3dDazzleTypeName Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dDazzleTypeName
                {
                    TypeName = reader.ReadFixedLengthString((int)context.CurrentEndPosition - (int)reader.BaseStream.Position)
                };

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.Write(TypeName);
        }
    }
}
