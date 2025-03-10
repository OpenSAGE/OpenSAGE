using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dTextureIds(IReadOnlyList<uint?> Items)
    : W3dListChunk<uint?>(W3dChunkType.W3D_CHUNK_TEXTURE_IDS, Items)
{
    internal static W3dTextureIds Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            var items = ParseItems(reader, context, r =>
            {
                var textureId = r.ReadInt32();
                return textureId != -1
                    ? (uint)textureId
                    : null;
            });

            return new W3dTextureIds(items);
        });
    }

    protected override void WriteItem(BinaryWriter writer, uint? item)
    {
        writer.Write(item != null ? (int)item.Value : -1);
    }
}

public sealed record W3dTextureStage(
    W3dTextureIds TextureIds,
    W3dVector2List? TexCoords,
    W3dVectorUInt32[]? PerFaceTexCoordIds) : W3dContainerChunk(W3dChunkType.W3D_CHUNK_TEXTURE_STAGE)
{
    internal static W3dTextureStage Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            W3dTextureIds? textureIds = null;
            W3dVector2List? texCoords = null;
            W3dVectorUInt32[]? perFaceTexCoordIds = null;

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_TEXTURE_IDS:
                        textureIds = W3dTextureIds.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_PER_FACE_TEXCOORD_IDS:
                        perFaceTexCoordIds = new W3dVectorUInt32[header.ChunkSize / W3dVectorUInt32.SizeInBytes];
                        for (var count = 0; count < perFaceTexCoordIds.Length; count++)
                        {
                            perFaceTexCoordIds[count] = W3dVectorUInt32.Parse(reader);
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_STAGE_TEXCOORDS:
                        texCoords = W3dVector2List.Parse(reader, context, chunkType);
                        break;

                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            if (textureIds is null)
            {
                throw new InvalidDataException("textureIds should never be null");
            }

            return new W3dTextureStage(textureIds, texCoords, perFaceTexCoordIds);
        });
    }

    protected override IEnumerable<W3dChunk> GetSubChunksOverride()
    {
        yield return TextureIds;

        if (TexCoords != null)
        {
            yield return TexCoords;
        }
    }
}
