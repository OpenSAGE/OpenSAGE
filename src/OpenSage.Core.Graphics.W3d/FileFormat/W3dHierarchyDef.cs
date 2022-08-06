using System.Collections.Generic;
using System.IO;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dHierarchyDef : W3dContainerChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_HIERARCHY;

        public W3dHierarchy Header { get; private set; }
        public W3dPivots Pivots { get; private set; }
        public W3dPivotFixups PivotFixups { get; private set; }

        internal static W3dHierarchyDef Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dHierarchyDef();

                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_HIERARCHY_HEADER:
                            result.Header = W3dHierarchy.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_PIVOTS:
                            result.Pivots = W3dPivots.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_PIVOT_FIXUPS:
                            result.PivotFixups = W3dPivotFixups.Parse(reader, context);
                            break;

                        default:
                            throw CreateUnknownChunkException(chunkType);
                    }
                });

                return result;
            });
        }

        protected override IEnumerable<W3dChunk> GetSubChunksOverride()
        {
            yield return Header;
            yield return Pivots;

            if (PivotFixups != null)
            {
                yield return PivotFixups;
            }
        }
    }

    public sealed class W3dPivotFixups : W3dStructListChunk<W3dPivotFixups, Matrix4x3>
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_PIVOT_FIXUPS;

        internal static W3dPivotFixups Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseList(reader, context, r => r.ReadMatrix4x3());
        }

        protected override void WriteItem(BinaryWriter writer, in Matrix4x3 item)
        {
            writer.Write(item);
        }
    }
}
