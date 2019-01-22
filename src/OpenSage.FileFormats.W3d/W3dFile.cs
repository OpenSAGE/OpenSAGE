using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenSage.Data.W3d
{
    public sealed class W3dFile
    {
        public string FilePath { get; private set; }

        public List<W3dChunk> Chunks { get; } = new List<W3dChunk>();

        public W3dHierarchyDef GetHierarchy() => Chunks.OfType<W3dHierarchyDef>().FirstOrDefault();

        public W3dHLod GetHLod() => Chunks.OfType<W3dHLod>().FirstOrDefault();

        public IReadOnlyList<W3dMesh> GetMeshes() => Chunks.OfType<W3dMesh>().ToList();

        public IReadOnlyList<W3dAnimation> GetAnimations() => Chunks.OfType<W3dAnimation>().ToList();

        public IReadOnlyList<W3dCompressedAnimation> GetCompressedAnimations() => Chunks.OfType<W3dCompressedAnimation>().ToList();

        public static W3dFile FromStream(Stream stream, string filePath)
        {
            using (var reader = new BinaryReader(stream, Encoding.ASCII, true))
            {
                return Parse(reader, filePath);
            }
        }

        private static W3dFile Parse(BinaryReader reader, string filePath)
        {
            var context = new W3dParseContext();

            context.PushChunk(nameof(W3dFile), reader.BaseStream.Length);

            var result = new W3dFile
            {
                FilePath = filePath
            };

            W3dContainerChunk.ParseChunks(reader, reader.BaseStream.Length, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_MESH:
                        result.Chunks.Add(W3dMesh.Parse(reader, context));
                        break;

                    case W3dChunkType.W3D_CHUNK_BOX:
                        result.Chunks.Add(W3dBox.Parse(reader, context));
                        break;

                    case W3dChunkType.W3D_CHUNK_HIERARCHY:
                        result.Chunks.Add(W3dHierarchyDef.Parse(reader, context));
                        break;

                    case W3dChunkType.W3D_CHUNK_HLOD:
                        result.Chunks.Add(W3dHLod.Parse(reader, context));
                        break;

                    case W3dChunkType.W3D_CHUNK_ANIMATION:
                        result.Chunks.Add(W3dAnimation.Parse(reader, context));
                        break;

                    case W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION:
                        result.Chunks.Add(W3dCompressedAnimation.Parse(reader, context));
                        break;

                    case W3dChunkType.W3D_CHUNK_EMITTER:
                        result.Chunks.Add(W3dEmitter.Parse(reader, context));
                        break;

                    case W3dChunkType.W3D_CHUNK_AGGREGATE:
                        result.Chunks.Add(W3dAggregate.Parse(reader, context));
                        break;

                    case W3dChunkType.W3D_CHUNK_RING:
                        result.Chunks.Add(W3dRing.Parse(reader, context));
                        break;

                    case W3dChunkType.W3D_CHUNK_SPHERE:
                        result.Chunks.Add(W3dSphere.Parse(reader, context));
                        break;

                    case W3dChunkType.W3D_CHUNK_HMODEL:
                        result.Chunks.Add(W3dHModel.Parse(reader, context));
                        break;

                    case W3dChunkType.W3D_CHUNK_DAZZLE:
                        result.Chunks.Add(W3dDazzle.Parse(reader, context));
                        break;

                    case W3dChunkType.W3D_CHUNK_COLLECTION:
                        result.Chunks.Add(W3dCollection.Parse(reader, context));
                        break;

                    case W3dChunkType.W3D_CHUNK_SOUNDROBJ:
                        result.Chunks.Add(W3dSoundRObj.Parse(reader, context));
                        break;

                    case W3dChunkType.W3D_CHUNK_MORPH_ANIMATION:
                        result.Chunks.Add(W3dMorphAnimation.Parse(reader, context));
                        break;

                    default:
                        throw W3dContainerChunk.CreateUnknownChunkException(chunkType);
                }
            });

            context.PopAsset();

            return result;
        }

        public void WriteTo(Stream stream)
        {
            using (var writer = new BinaryWriter(stream))
            {
                foreach (var chunk in Chunks)
                {
                    chunk.WriteTo(writer);
                }
            }
        }
    }
}
