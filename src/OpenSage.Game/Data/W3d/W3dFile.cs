using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenSage.Data.W3d
{
    public sealed class W3dFile
    {
        public W3dMesh[] Meshes { get; private set; }
        
        public W3dHierarchyDef Hierarchy { get; private set; }

        public W3dHLod HLod { get; private set; }

        public W3dAnimation[] Animations { get; private set; }

        public static W3dFile FromStream(Stream stream)
        {
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
                return Parse(reader);
        }

        private static W3dFile Parse(BinaryReader reader)
        {
            var meshes = new List<W3dMesh>();
            W3dHierarchyDef hierarchy = null;
            W3dHLod hlod = null;
            var animations = new List<W3dAnimation>();

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
                        if (hierarchy != null)
                        {
                            throw new InvalidDataException();
                        }
                        hierarchy = W3dHierarchyDef.Parse(reader, currentChunk.ChunkSize);
                        break;

                    case W3dChunkType.W3D_CHUNK_HLOD:
                        if (hlod != null)
                        {
                            throw new InvalidDataException();
                        }
                        hlod = W3dHLod.Parse(reader, currentChunk.ChunkSize);
                        break;

                    case W3dChunkType.W3D_CHUNK_ANIMATION:
                        animations.Add(W3dAnimation.Parse(reader, currentChunk.ChunkSize));
                        break;

                    // TODO

                    default:
                        reader.ReadBytes((int) currentChunk.ChunkSize);
                        break;
                }
            } while (loadedSize < reader.BaseStream.Length);

            return new W3dFile
            {
                Meshes = meshes.ToArray(),
                Hierarchy = hierarchy,
                HLod = hlod,
                Animations = animations.ToArray()
                // TODO
            };
        }
    }
}
