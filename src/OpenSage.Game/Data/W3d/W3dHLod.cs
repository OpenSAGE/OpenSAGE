using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dHLod : W3dChunk
    {
        public W3dHLodHeader Header { get; private set; }

        public IReadOnlyList<W3dHLodArray> Lods { get; private set; }

        public W3dHLodArray Aggregate { get; private set; }

        public static W3dHLod Parse(BinaryReader reader, uint chunkSize)
        {
            var lods = new List<W3dHLodArray>();

            var finalResult = ParseChunk<W3dHLod>(reader, chunkSize, (result, header) =>
            {
                switch (header.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_HLOD_HEADER:
                        result.Header = W3dHLodHeader.Parse(reader);
                        result.Lods = new W3dHLodArray[result.Header.LodCount];
                        break;

                    case W3dChunkType.W3D_CHUNK_HLOD_LOD_ARRAY:
                        lods.Add(W3dHLodArray.Parse(reader, header.ChunkSize));
                        break;

                    case W3dChunkType.W3D_CHUNK_HLOD_AGGREGATE_ARRAY:
                        if (result.Aggregate != null)
                        {
                            throw new InvalidDataException();
                        }
                        result.Aggregate = W3dHLodArray.Parse(reader, header.ChunkSize);
                        break;

                    default:
                        throw CreateUnknownChunkException(header);
                }
            });

            finalResult.Lods = lods;

            return finalResult;
        }
    }
}
