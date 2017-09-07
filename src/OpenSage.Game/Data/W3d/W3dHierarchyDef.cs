using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dHierarchyDef : W3dChunk
    {
        public W3dHierarchy Header { get; private set; }
        public W3dPivot[] Pivots { get; private set; }
        //public float[] RotateMatrix { get; private set; }
        //public uint[] Visible { get; private set; }

        public static W3dHierarchyDef Parse(BinaryReader reader, uint chunkSize)
        {
            return ParseChunk<W3dHierarchyDef>(reader, chunkSize, (result, header) =>
            {
                switch (header.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_HIERARCHY_HEADER:
                        result.Header = W3dHierarchy.Parse(reader);
                        result.Pivots = new W3dPivot[result.Header.NumPivots];
                        break;

                    case W3dChunkType.W3D_CHUNK_PIVOTS:
                        for (var count = 0; count < result.Header.NumPivots; count++)
                        {
                            result.Pivots[count] = W3dPivot.Parse(reader);
                        }
                        break;

                    default:
                        reader.ReadBytes((int) header.ChunkSize);
                        break;
                }
            });
        }
    }
}
