using System.IO;

namespace OpenZH.Data.W3d
{
    public sealed class W3dMesh
    {
        public W3dMeshHeader3 Header { get; private set; }
        public W3dVector[] Vertices { get; private set; }

        // TODO

        public static W3dMesh Parse(BinaryReader reader, uint chunkSize)
        {
            var result = new W3dMesh();

            uint loadedSize = 0;

            do
            {
                loadedSize += W3dChunkHeader.SizeInBytes;
                var currentChunk = W3dChunkHeader.Parse(reader);

                loadedSize += currentChunk.ChunkSize;

                switch (currentChunk.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_MESH_HEADER3:
                        result.Header = W3dMeshHeader3.Parse(reader);
                        result.Vertices = new W3dVector[result.Header.NumVertices];
                        // TODO
                        break;

                    case W3dChunkType.W3D_CHUNK_VERTICES:
                        for (var count = 0; count < result.Header.NumVertices; count++)
                        {
                            result.Vertices[count] = W3dVector.Parse(reader);
                        }
                        break;

                    // TODO

                    default:
                        reader.ReadBytes((int) currentChunk.ChunkSize);
                        break;
                }

            } while (loadedSize < chunkSize);

            return result;
        }
    }
}
