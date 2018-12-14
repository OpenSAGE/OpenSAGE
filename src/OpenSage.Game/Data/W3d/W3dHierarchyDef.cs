using System.IO;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.Mathematics;

namespace OpenSage.Data.W3d
{
    public sealed class W3dHierarchyDef : W3dChunk
    {
        public W3dHierarchy Header { get; private set; }
        public W3dPivot[] Pivots { get; private set; }
        public Matrix4x3[] PivotFixups { get; private set; }

        internal static W3dHierarchyDef Parse(BinaryReader reader, uint chunkSize)
        {
            return ParseChunk<W3dHierarchyDef>(reader, chunkSize, (result, header) =>
            {
                switch (header.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_HIERARCHY_HEADER:
                        result.Header = W3dHierarchy.Parse(reader);
                        result.Pivots = new W3dPivot[result.Header.NumPivots];
                        result.PivotFixups = new Matrix4x3[result.Header.NumPivots];
                        break;

                    case W3dChunkType.W3D_CHUNK_PIVOTS:
                        for (var count = 0; count < result.Header.NumPivots; count++)
                        {
                            result.Pivots[count] = W3dPivot.Parse(reader);
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_PIVOT_FIXUPS:
                        for (var count = 0; count < result.Header.NumPivots; count++)
                        {
                            result.PivotFixups[count] = reader.ReadMatrix4x3();
                        }
                        break;

                    default:
                        throw CreateUnknownChunkException(header);
                }
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_HIERARCHY_HEADER, false, () => Header.WriteTo(writer));

            WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_PIVOTS, false, () =>
            {
                for (var i = 0; i < Header.NumPivots; i++)
                {
                    Pivots[i].WriteTo(writer);
                }
            });

            WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_PIVOT_FIXUPS, false, () =>
            {
                for (var i = 0; i < Header.NumPivots; i++)
                {
                    writer.Write(PivotFixups[i]);
                }
            });
        }
    }
}
