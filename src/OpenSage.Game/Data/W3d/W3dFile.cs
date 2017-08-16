using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenSage.Data.W3d
{
    public sealed class W3dFile
    {
        public uint ChunkCount { get; private set; }

        public W3dMesh[] Meshes { get; private set; }
        
        public W3dHierarchyDef[] Hierarchies { get; private set; }

        public static W3dFile FromStream(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
                return Parse(reader);
        }

        public static W3dFile Parse(BinaryReader reader)
        {
            var meshes = new List<W3dMesh>();
            var hierarchies = new List<W3dHierarchyDef>();

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

                    case W3dChunkType.W3D_CHUNK_HIERARCHY:
                        hierarchies.Add(W3dHierarchyDef.Parse(reader, currentChunk.ChunkSize));
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
                Meshes = meshes.ToArray(),
                Hierarchies = hierarchies.ToArray()
                // TODO
            };
        }
    }
}
