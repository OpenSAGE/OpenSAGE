using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenSage.Data.W3d
{
    public sealed class W3dFile : W3dChunk
    {
        public string FilePath { get; private set; }

        public IReadOnlyList<W3dMesh> Meshes { get; private set; }

        public IReadOnlyList<W3dBox> Boxes { get; private set; }
        
        public W3dHierarchyDef Hierarchy { get; private set; }

        public W3dHLod HLod { get; private set; }

        public IReadOnlyList<W3dAnimation> Animations { get; private set; }

        public IReadOnlyList<W3dCompressedAnimation> CompressedAnimations { get; private set; }

        public IReadOnlyList<W3dEmitter> Emitters { get; private set; }

        public static W3dFile FromFileSystemEntry(FileSystemEntry entry)
        {
            using (var stream = entry.Open())
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
                return Parse(reader, entry.FilePath);
        }

        private static W3dFile Parse(BinaryReader reader, string filePath)
        {
            var meshes = new List<W3dMesh>();
            var boxes = new List<W3dBox>();
            W3dHierarchyDef hierarchy = null;
            W3dHLod hlod = null;
            var animations = new List<W3dAnimation>();
            var compressedAnimations = new List<W3dCompressedAnimation>();
            var emitters = new List<W3dEmitter>();

            var result = ParseChunk<W3dFile>(reader, (uint) reader.BaseStream.Length, (x, header) =>
            {
                switch (header.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_MESH:
                        meshes.Add(W3dMesh.Parse(reader, header.ChunkSize));
                        break;

                    case W3dChunkType.W3D_CHUNK_BOX:
                        boxes.Add(W3dBox.Parse(reader));
                        break;

                    case W3dChunkType.W3D_CHUNK_HIERARCHY:
                        if (hierarchy != null)
                        {
                            throw new InvalidDataException();
                        }
                        hierarchy = W3dHierarchyDef.Parse(reader, header.ChunkSize);
                        break;

                    case W3dChunkType.W3D_CHUNK_HLOD:
                        if (hlod != null)
                        {
                            throw new InvalidDataException();
                        }
                        hlod = W3dHLod.Parse(reader, header.ChunkSize);
                        break;

                    case W3dChunkType.W3D_CHUNK_ANIMATION:
                        animations.Add(W3dAnimation.Parse(reader, header.ChunkSize));
                        break;

                    case W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION:
                        compressedAnimations.Add(W3dCompressedAnimation.Parse(reader, header.ChunkSize));
                        break;

                    case W3dChunkType.W3D_CHUNK_EMITTER:
                        emitters.Add(W3dEmitter.Parse(reader, header.ChunkSize));
                        break;

                    default:
                        throw new InvalidDataException();
                }
            });

            result.FilePath = filePath;
            result.Meshes = meshes;
            result.Boxes = boxes;
            result.Hierarchy = hierarchy;
            result.HLod = hlod;
            result.Animations = animations;
            result.CompressedAnimations = compressedAnimations;
            result.Emitters = emitters;

            return result;
        }
    }
}
