using System.IO;

namespace OpenZH.Data.W3d
{
    public sealed class W3dTextureStage
    {
        public uint[] TextureIds { get; private set; }

        public W3dTexCoord[] TexCoords { get; private set; }

        public W3dVectorUInt32[] PerFaceTexCoordIds { get; private set; }

        public static W3dTextureStage Parse(BinaryReader reader, uint chunkSize)
        {
            var result = new W3dTextureStage();

            uint loadedSize = 0;

            do
            {
                loadedSize += W3dChunkHeader.SizeInBytes;
                var currentChunk = W3dChunkHeader.Parse(reader);

                loadedSize += currentChunk.ChunkSize;

                switch (currentChunk.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_TEXTURE_IDS:
                        result.TextureIds = new uint[currentChunk.ChunkSize / sizeof(uint)];
                        for (var count = 0; count < result.TextureIds.Length; count++)
                        {
                            result.TextureIds[count] = reader.ReadUInt32();
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_PER_FACE_TEXCOORD_IDS:
                        result.PerFaceTexCoordIds = new W3dVectorUInt32[currentChunk.ChunkSize / W3dVectorUInt32.SizeInBytes];
                        for (var count = 0; count < result.PerFaceTexCoordIds.Length; count++)
                        {
                            result.PerFaceTexCoordIds[count] = W3dVectorUInt32.Parse(reader);
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_STAGE_TEXCOORDS:
                        result.TexCoords = new W3dTexCoord[currentChunk.ChunkSize / W3dTexCoord.SizeInBytes];
                        for (var count = 0; count < result.TexCoords.Length; count++)
                        {
                            result.TexCoords[count] = W3dTexCoord.Parse(reader);
                        }
                        break;

                    default:
                        throw new InvalidDataException($"Unknown chunk type: {currentChunk.ChunkType}");
                }
            } while (loadedSize < chunkSize);

            return result;
        }
    }
}
