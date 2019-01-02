using System.IO;
using System.Collections.Generic;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dAggregateInfo : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_AGGREGATE_INFO;

        public string BaseModelName { get; private set; }

        public uint SubObjectCount { get; private set; }

        public List<W3dAggregateSubObject> SubObjects { get; private set; }

        internal static W3dAggregateInfo Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dAggregateInfo
                {
                    BaseModelName = reader.ReadFixedLengthString(W3dConstants.NameLength * 2),
                    SubObjectCount = reader.ReadUInt32()
                };

                result.SubObjects = new List<W3dAggregateSubObject>();
                for (var i = 0; i < result.SubObjectCount; i++)
                {
                    result.SubObjects.Add(W3dAggregateSubObject.Parse(reader));
                }

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            writer.WriteFixedLengthString(BaseModelName, W3dConstants.NameLength * 2);
            writer.Write(SubObjectCount);
            SubObjects.ForEach(delegate (W3dAggregateSubObject subObject)
            {
                subObject.WriteTo(writer);
            });
        }
    }
}
