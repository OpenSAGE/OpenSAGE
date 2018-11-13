using OpenSage.Data.Utilities.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace OpenSage.Data.W3d
{
    public sealed class W3dMaterialPass : W3dChunk
    {
        public uint[] VertexMaterialIds { get; private set; }

        public uint[] ShaderIds { get; private set; }

        public uint? ShaderMaterialId { get; private set; }
        public uint[] ShaderMaterialIds { get; private set; }

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

        /// <summary>
        /// Only present when using shader materials.
        /// </summary>
        public Vector2[] TexCoords { get; private set; }

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

                    case W3dChunkType.W3D_CHUNK_SHADER_MATERIAL_ID:
                        result.ShaderMaterialIds = new uint[header.ChunkSize / sizeof(uint)];
                        for (var count = 0; count < result.ShaderMaterialIds.Length; count++)
                        {
                            result.ShaderMaterialIds[count] = reader.ReadUInt32();
                        }
                        result.ShaderMaterialId = result.ShaderMaterialIds[0];
                        break;

                    // Normally this appears inside W3dTextureStage, but it can also
                    // appear directly under W3dMaterialPass if using shader materials.
                    case W3dChunkType.W3D_CHUNK_STAGE_TEXCOORDS:
                        result.TexCoords = new Vector2[header.ChunkSize / (sizeof(float) * 2)];
                        for (var count = 0; count < result.TexCoords.Length; count++)
                        {
                            result.TexCoords[count] = reader.ReadVector2();
                        }
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
