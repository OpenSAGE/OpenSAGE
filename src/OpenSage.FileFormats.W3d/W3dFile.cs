using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dFile
    {
        public string FilePath { get; private set; }

        public List<W3dChunk> Chunks { get; } = new List<W3dChunk>();

        public List<W3dChunk> RenderableObjects { get; } = new();

        public Dictionary<string, W3dChunk> RenderableObjectsByName = new();

        public W3dHierarchyDef Hierarchy { get; private set; }

        public W3dHLod HLod { get; private set; }

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
                        var w3dMesh = W3dMesh.Parse(reader, context);
                        result.Chunks.Add(w3dMesh);
                        result.RenderableObjects.Add(w3dMesh);
                        result.RenderableObjectsByName.Add(
                            $"{w3dMesh.Header.ContainerName}.{w3dMesh.Header.MeshName}",
                            w3dMesh);
                        break;

                    case W3dChunkType.W3D_CHUNK_BOX:
                        var w3dBox = W3dBox.Parse(reader, context);
                        result.Chunks.Add(w3dBox);
                        result.RenderableObjects.Add(w3dBox);
                        result.RenderableObjectsByName.Add(w3dBox.Name, w3dBox);
                        break;

                    case W3dChunkType.W3D_CHUNK_HIERARCHY:
                        var w3dHierarchy = W3dHierarchyDef.Parse(reader, context);
                        result.Chunks.Add(w3dHierarchy);
                        result.Hierarchy = w3dHierarchy;
                        break;

                    case W3dChunkType.W3D_CHUNK_HLOD:
                        var w3dHLod = W3dHLod.Parse(reader, context);
                        result.Chunks.Add(w3dHLod);
                        result.HLod = w3dHLod;
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
