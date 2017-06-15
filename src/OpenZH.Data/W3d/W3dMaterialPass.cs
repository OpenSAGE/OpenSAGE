using System.IO;

namespace OpenZH.Data.W3d
{
    public sealed class W3dMaterialPass
    {
        public uint[] VertexMaterialIds { get; private set; }

        public uint[] ShaderIds { get; private set; }

        public W3dRgba[] Dcg { get; private set; }
        public W3dRgba[] Dig{ get; private set; }
        public W3dRgba[] Scg{ get; private set; }

        public uint StageCount { get; private set; }

        public W3dTextureStage[] TextureStages { get; } = new W3dTextureStage[2];

        public static W3dMaterialPass Parse(BinaryReader reader, uint chunkSize)
        {
            var result = new W3dMaterialPass();

            uint loadedSize = 0;

            do
            {
                loadedSize += W3dChunkHeader.SizeInBytes;
                var currentChunk = W3dChunkHeader.Parse(reader);

                loadedSize += currentChunk.ChunkSize;

                switch (currentChunk.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_VERTEX_MATERIAL_IDS:
                        result.VertexMaterialIds = new uint[currentChunk.ChunkSize / sizeof(uint)];
                        for (var count = 0; count < result.VertexMaterialIds.Length; count++)
                        {
                            result.VertexMaterialIds[count] = reader.ReadUInt32();
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_SHADER_IDS:
                        result.ShaderIds = new uint[currentChunk.ChunkSize / sizeof(uint)];
                        for (var count = 0; count < result.ShaderIds.Length; count++)
                        {
                            result.ShaderIds[count] = reader.ReadUInt32();
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_DCG:
                        result.Dcg = new W3dRgba[currentChunk.ChunkSize / W3dRgba.SizeInBytes];
                        for (var count = 0; count < result.Dcg.Length; count++)
                        {
                            result.Dcg[count] = W3dRgba.Parse(reader);
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_DIG:
                        result.Dig = new W3dRgba[currentChunk.ChunkSize / W3dRgba.SizeInBytes];
                        for (var count = 0; count < result.Dig.Length; count++)
                        {
                            result.Dig[count] = W3dRgba.Parse(reader);
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_SCG:
                        result.Scg = new W3dRgba[currentChunk.ChunkSize / W3dRgba.SizeInBytes];
                        for (var count = 0; count < result.Scg.Length; count++)
                        {
                            result.Scg[count] = W3dRgba.Parse(reader);
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_TEXTURE_STAGE:
                        result.StageCount++;
                        result.TextureStages[result.StageCount - 1] = W3dTextureStage.Parse(reader, currentChunk.ChunkSize);
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
