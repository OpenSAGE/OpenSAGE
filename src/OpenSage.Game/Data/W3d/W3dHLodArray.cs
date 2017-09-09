using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dHLodArray : W3dChunk
    {
        public W3dHLodArrayHeader Header { get; private set; }

        public W3dHLodSubObject[] SubObjects { get; private set; }

        public static W3dHLodArray Parse(BinaryReader reader, uint chunkSize)
        {
            var currentSubObjectIndex = 0;

            return ParseChunk<W3dHLodArray>(reader, chunkSize, (result, header) =>
            {
                switch (header.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_HLOD_SUB_OBJECT_ARRAY_HEADER:
                        result.Header = W3dHLodArrayHeader.Parse(reader);
                        result.SubObjects = new W3dHLodSubObject[result.Header.ModelCount];
                        break;

                    case W3dChunkType.W3D_CHUNK_HLOD_SUB_OBJECT:
                        result.SubObjects[currentSubObjectIndex] = W3dHLodSubObject.Parse(reader);
                        currentSubObjectIndex++;
                        break;

                    default:
                        throw CreateUnknownChunkException(header);
                }
            });
        }
    }
}
