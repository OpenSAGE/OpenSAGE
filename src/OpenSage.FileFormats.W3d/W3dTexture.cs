using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dTexture : W3dContainerChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_TEXTURE;

        public W3dTextureName Name { get; private set; }

        public W3dTextureInfo TextureInfo { get; private set; }

        internal static W3dTexture Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dTexture();

                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_TEXTURE_NAME:
                            result.Name = W3dTextureName.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_TEXTURE_INFO:
                            result.TextureInfo = W3dTextureInfo.Parse(reader, context);
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
            yield return Name;

            if (TextureInfo != null)
            {
                yield return TextureInfo;
            }
        }
    }
}
