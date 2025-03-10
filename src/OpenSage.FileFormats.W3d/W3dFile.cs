using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dFile(
    string FilePath,
    List<W3dChunk> Chunks,
    List<W3dChunk> RenderableObjects,
    Dictionary<string, W3dChunk> RenderableObjectsByName,
    W3dHierarchyDef? Hierarchy,
    W3dHLod? HLod)
{
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

        List<W3dChunk> chunks = [];
        List<W3dChunk> renderableObjects = [];
        Dictionary<string, W3dChunk> renderableObjectsByName = [];
        W3dHierarchyDef? hierarchy = null;
        W3dHLod? hLod = null;

        W3dContainerChunk.ParseChunks(reader, reader.BaseStream.Length, chunkType =>
        {
            switch (chunkType)
            {
                case W3dChunkType.W3D_CHUNK_MESH:
                    var w3dMesh = W3dMesh.Parse(reader, context);
                    chunks.Add(w3dMesh);
                    renderableObjects.Add(w3dMesh);
                    renderableObjectsByName.Add(
                        $"{w3dMesh.Header.ContainerName}.{w3dMesh.Header.MeshName}",
                        w3dMesh);
                    break;

                case W3dChunkType.W3D_CHUNK_BOX:
                    var w3dBox = W3dBox.Parse(reader, context);
                    chunks.Add(w3dBox);
                    renderableObjects.Add(w3dBox);
                    renderableObjectsByName.Add(w3dBox.Name, w3dBox);
                    break;

                case W3dChunkType.W3D_CHUNK_HIERARCHY:
                    var w3dHierarchy = W3dHierarchyDef.Parse(reader, context);
                    chunks.Add(w3dHierarchy);
                    hierarchy = w3dHierarchy;
                    break;

                case W3dChunkType.W3D_CHUNK_HLOD:
                    var w3dHLod = W3dHLod.Parse(reader, context);
                    chunks.Add(w3dHLod);
                    hLod = w3dHLod;
                    break;

                case W3dChunkType.W3D_CHUNK_ANIMATION:
                    chunks.Add(W3dAnimation.Parse(reader, context));
                    break;

                case W3dChunkType.W3D_CHUNK_COMPRESSED_ANIMATION:
                    chunks.Add(W3dCompressedAnimation.Parse(reader, context));
                    break;

                case W3dChunkType.W3D_CHUNK_EMITTER:
                    chunks.Add(W3dEmitter.Parse(reader, context));
                    break;

                case W3dChunkType.W3D_CHUNK_AGGREGATE:
                    chunks.Add(W3dAggregate.Parse(reader, context));
                    break;

                case W3dChunkType.W3D_CHUNK_RING:
                    chunks.Add(W3dRing.Parse(reader, context));
                    break;

                case W3dChunkType.W3D_CHUNK_SPHERE:
                    chunks.Add(W3dSphere.Parse(reader, context));
                    break;

                case W3dChunkType.W3D_CHUNK_HMODEL:
                    chunks.Add(W3dHModel.Parse(reader, context));
                    break;

                case W3dChunkType.W3D_CHUNK_DAZZLE:
                    chunks.Add(W3dDazzle.Parse(reader, context));
                    break;

                case W3dChunkType.W3D_CHUNK_COLLECTION:
                    chunks.Add(W3dCollection.Parse(reader, context));
                    break;

                case W3dChunkType.W3D_CHUNK_SOUNDROBJ:
                    chunks.Add(W3dSoundRObj.Parse(reader, context));
                    break;

                case W3dChunkType.W3D_CHUNK_MORPH_ANIMATION:
                    chunks.Add(W3dMorphAnimation.Parse(reader, context));
                    break;

                default:
                    throw W3dContainerChunk.CreateUnknownChunkException(chunkType);
            }
        });

        context.PopAsset();

        return new W3dFile(filePath, chunks, renderableObjects, renderableObjectsByName, hierarchy, hLod);
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
