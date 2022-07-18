using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dTextureIds : W3dListChunk<W3dTextureIds, uint?>
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_TEXTURE_IDS;

        internal static W3dTextureIds Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseList(reader, context, r =>
            {
                var textureId = r.ReadInt32();
                return textureId != -1
                    ? (uint) textureId
                    : (uint?) null;
            });
        }

        protected override void WriteItem(BinaryWriter writer, uint? item)
        {
            writer.Write(item != null ? (int) item.Value : -1);
        }
    }

    public sealed class W3dTextureStage : W3dContainerChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_TEXTURE_STAGE;

        public W3dTextureIds TextureIds { get; private set; }

        public W3dVector2List TexCoords { get; private set; }

        public W3dVectorUInt32[] PerFaceTexCoordIds { get; private set; }

        internal static W3dTextureStage Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dTextureStage();

                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_TEXTURE_IDS:
                            result.TextureIds = W3dTextureIds.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_PER_FACE_TEXCOORD_IDS:
                            result.PerFaceTexCoordIds = new W3dVectorUInt32[header.ChunkSize / W3dVectorUInt32.SizeInBytes];
                            for (var count = 0; count < result.PerFaceTexCoordIds.Length; count++)
                            {
                                result.PerFaceTexCoordIds[count] = W3dVectorUInt32.Parse(reader);
                            }
                            break;

                        case W3dChunkType.W3D_CHUNK_STAGE_TEXCOORDS:
                            result.TexCoords = W3dVector2List.Parse(reader, context, chunkType);
                            break;

                        default:
                            throw CreateUnknownChunkException(chunkType);
                    }
                });

                return result;
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
}
