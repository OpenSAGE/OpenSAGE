using System.IO;
using OpenZH.Data.Utilities.Extensions;

namespace OpenZH.Data.W3d
{
    public sealed class W3dTexture
    {
        public string Name { get; private set; }

        public W3dTextureInfo TextureInfo { get; private set; }

        public static W3dTexture Parse(BinaryReader reader, uint chunkSize)
        {
            var result = new W3dTexture();

            uint loadedSize = 0;

            do
            {
                loadedSize += W3dChunkHeader.SizeInBytes;
                var currentChunk = W3dChunkHeader.Parse(reader);

                loadedSize += currentChunk.ChunkSize;

                switch (currentChunk.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_TEXTURE_NAME:
                        result.Name = reader.ReadFixedLengthString((int) currentChunk.ChunkSize);
                        break;

                    case W3dChunkType.W3D_CHUNK_TEXTURE_INFO:
                        result.TextureInfo = W3dTextureInfo.Parse(reader);
                        break;

                    default:
                        reader.ReadBytes((int) currentChunk.ChunkSize);
                        break;
                }
            } while (loadedSize < chunkSize);

            return result;
        }
    }
}
