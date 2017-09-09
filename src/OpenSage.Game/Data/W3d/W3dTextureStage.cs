using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dTextureStage : W3dChunk
    {
        public uint[] TextureIds { get; private set; }

        public W3dTexCoord[] TexCoords { get; private set; }

        public W3dVectorUInt32[] PerFaceTexCoordIds { get; private set; }

        public static W3dTextureStage Parse(BinaryReader reader, uint chunkSize)
        {
            return ParseChunk<W3dTextureStage>(reader, chunkSize, (result, header) =>
            {
                switch (header.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_TEXTURE_IDS:
                        result.TextureIds = new uint[header.ChunkSize / sizeof(uint)];
                        for (var count = 0; count < result.TextureIds.Length; count++)
                        {
                            result.TextureIds[count] = reader.ReadUInt32();
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_PER_FACE_TEXCOORD_IDS:
                        result.PerFaceTexCoordIds = new W3dVectorUInt32[header.ChunkSize / W3dVectorUInt32.SizeInBytes];
                        for (var count = 0; count < result.PerFaceTexCoordIds.Length; count++)
                        {
                            result.PerFaceTexCoordIds[count] = W3dVectorUInt32.Parse(reader);
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_STAGE_TEXCOORDS:
                        result.TexCoords = new W3dTexCoord[header.ChunkSize / W3dTexCoord.SizeInBytes];
                        for (var count = 0; count < result.TexCoords.Length; count++)
                        {
                            result.TexCoords[count] = W3dTexCoord.Parse(reader);
                        }
                        break;

                    default:
                        throw CreateUnknownChunkException(header);
                }
            });
        }
    }
}
