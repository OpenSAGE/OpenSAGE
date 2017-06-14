using System.Collections.Generic;
using System.IO;

namespace OpenZH.Data.W3d
{
    public sealed class W3dFile
    {
        public uint ChunkCount { get; private set; }

        public W3dMesh[] Meshes { get; private set; }
        // TODO

        public static W3dFile Parse(BinaryReader reader)
        {
            var meshes = new List<W3dMesh>();

            uint chunkCount = 0;
            uint loadedSize = 0;

            do
            {
                chunkCount++;
                loadedSize += W3dChunkHeader.SizeInBytes;

                var currentChunk = W3dChunkHeader.Parse(reader);
                loadedSize += currentChunk.ChunkSize;

                switch (currentChunk.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_MESH:
                        meshes.Add(W3dMesh.Parse(reader, currentChunk.ChunkSize));
                        break;

                    // TODO

                    default:
                        reader.ReadBytes((int) currentChunk.ChunkSize);
                        break;
                }
            } while (loadedSize < reader.BaseStream.Length);

            return new W3dFile
            {
                ChunkCount = chunkCount,
                Meshes = meshes.ToArray()
                // TODO
            };
        }
    }
}
