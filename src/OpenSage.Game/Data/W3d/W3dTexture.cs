﻿using System.IO;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dTexture : W3dChunk
    {
        public string Name { get; private set; }

        public W3dTextureInfo TextureInfo { get; private set; }

        internal static W3dTexture Parse(BinaryReader reader, uint chunkSize)
        {
            return ParseChunk<W3dTexture>(reader, chunkSize, (result, header) =>
            {
                switch (header.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_TEXTURE_NAME:
                        result.Name = reader.ReadFixedLengthString((int) header.ChunkSize);
                        break;

                    case W3dChunkType.W3D_CHUNK_TEXTURE_INFO:
                        result.TextureInfo = W3dTextureInfo.Parse(reader);
                        break;

                    default:
                        throw CreateUnknownChunkException(header);
                }
            });
        }

        internal void WriteTo(BinaryWriter writer)
        {
            WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_TEXTURE_NAME, false, () =>
            {
                writer.WriteFixedLengthString(Name, Name.Length + 1);
            });

            if (TextureInfo != null)
            {
                WriteChunkTo(writer, W3dChunkType.W3D_CHUNK_TEXTURE_INFO, false, () =>
                {
                    TextureInfo.WriteTo(writer);
                });
            }
        }
    }
}
