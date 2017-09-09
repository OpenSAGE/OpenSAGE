using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dMaterialPass : W3dChunk
    {
        public uint[] VertexMaterialIds { get; private set; }

        public uint[] ShaderIds { get; private set; }

        /// <summary>
        /// per-vertex diffuse color values
        /// </summary>
        public W3dRgba[] Dcg { get; private set; }

        /// <summary>
        /// per-vertex diffuse illumination values
        /// </summary>
        public W3dRgba[] Dig { get; private set; }

        /// <summary>
        /// per-vertex specular color values
        /// </summary>
        public W3dRgba[] Scg { get; private set; }

        public IReadOnlyList<W3dTextureStage> TextureStages { get; private set; }

        public static W3dMaterialPass Parse(BinaryReader reader, uint chunkSize)
        {
            var textureStages = new List<W3dTextureStage>();

            var r = ParseChunk<W3dMaterialPass>(reader, chunkSize, (result, header) =>
            {
                switch (header.ChunkType)
                {
                    case W3dChunkType.W3D_CHUNK_VERTEX_MATERIAL_IDS:
                        result.VertexMaterialIds = new uint[header.ChunkSize / sizeof(uint)];
                        for (var count = 0; count < result.VertexMaterialIds.Length; count++)
                        {
                            result.VertexMaterialIds[count] = reader.ReadUInt32();
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_SHADER_IDS:
                        result.ShaderIds = new uint[header.ChunkSize / sizeof(uint)];
                        for (var count = 0; count < result.ShaderIds.Length; count++)
                        {
                            result.ShaderIds[count] = reader.ReadUInt32();
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_DCG:
                        result.Dcg = new W3dRgba[header.ChunkSize / W3dRgba.SizeInBytes];
                        for (var count = 0; count < result.Dcg.Length; count++)
                        {
                            result.Dcg[count] = W3dRgba.Parse(reader);
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_DIG:
                        result.Dig = new W3dRgba[header.ChunkSize / W3dRgba.SizeInBytes];
                        for (var count = 0; count < result.Dig.Length; count++)
                        {
                            result.Dig[count] = W3dRgba.Parse(reader);
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_SCG:
                        result.Scg = new W3dRgba[header.ChunkSize / W3dRgba.SizeInBytes];
                        for (var count = 0; count < result.Scg.Length; count++)
                        {
                            result.Scg[count] = W3dRgba.Parse(reader);
                        }
                        break;

                    case W3dChunkType.W3D_CHUNK_TEXTURE_STAGE:
                        textureStages.Add(W3dTextureStage.Parse(reader, header.ChunkSize));
                        break;

                    default:
                        throw CreateUnknownChunkException(header);
                }
            });

            r.TextureStages = textureStages;

            return r;
        }
    }
}
