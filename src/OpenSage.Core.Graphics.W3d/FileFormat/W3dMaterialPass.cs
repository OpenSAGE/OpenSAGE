using System.Collections.Generic;
using System.IO;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dMaterialPass : W3dContainerChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_MATERIAL_PASS;

        public W3dUInt32List VertexMaterialIds { get; private set; }

        public W3dUInt32List ShaderIds { get; private set; }

        public W3dUInt32List ShaderMaterialIds { get; private set; }

        /// <summary>
        /// per-vertex diffuse color values
        /// </summary>
        public W3dRgbaList Dcg { get; private set; }

        /// <summary>
        /// per-vertex diffuse illumination values
        /// </summary>
        public W3dRgbaList Dig { get; private set; }

        /// <summary>
        /// per-vertex specular color values
        /// </summary>
        public W3dRgbaList Scg { get; private set; }

        public List<W3dTextureStage> TextureStages { get; } = new List<W3dTextureStage>();

        /// <summary>
        /// Only present when using shader materials.
        /// </summary>
        public W3dVector2List TexCoords { get; private set; }

        internal static W3dMaterialPass Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dMaterialPass();

                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_VERTEX_MATERIAL_IDS:
                            result.VertexMaterialIds = W3dUInt32List.Parse(reader, context, chunkType);
                            break;

                        case W3dChunkType.W3D_CHUNK_SHADER_IDS:
                            result.ShaderIds = W3dUInt32List.Parse(reader, context, chunkType);
                            break;

                        case W3dChunkType.W3D_CHUNK_DCG:
                            result.Dcg = W3dRgbaList.Parse(reader, context, chunkType);
                            break;

                        case W3dChunkType.W3D_CHUNK_DIG:
                            result.Dig = W3dRgbaList.Parse(reader, context, chunkType);
                            break;

                        case W3dChunkType.W3D_CHUNK_SCG:
                            result.Scg = W3dRgbaList.Parse(reader, context, chunkType);
                            break;

                        case W3dChunkType.W3D_CHUNK_TEXTURE_STAGE:
                            result.TextureStages.Add(W3dTextureStage.Parse(reader, context));
                            break;

                        case W3dChunkType.W3D_CHUNK_SHADER_MATERIAL_ID:
                            result.ShaderMaterialIds = W3dUInt32List.Parse(reader, context, chunkType);
                            break;

                        // Normally this appears inside W3dTextureStage, but it can also
                        // appear directly under W3dMaterialPass if using shader materials.
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
            if (VertexMaterialIds != null)
            {
                yield return VertexMaterialIds;
            }

            if (ShaderIds != null)
            {
                yield return ShaderIds;
            }

            if (Dcg != null)
            {
                yield return Dcg;
            }

            if (Dig != null)
            {
                yield return Dig;
            }

            if (Scg != null)
            {
                yield return Scg;
            }

            foreach (var textureStage in TextureStages)
            {
                yield return textureStage;
            }

            if (ShaderMaterialIds != null)
            {
                yield return ShaderMaterialIds;
            }

            if (TexCoords != null)
            {
                yield return TexCoords;
            }
        }
    }

    public sealed class W3dRgbaList : W3dStructListChunk<W3dRgbaList, ColorRgba>
    {
        private W3dChunkType _chunkType;

        public override W3dChunkType ChunkType => _chunkType;

        internal static W3dRgbaList Parse(BinaryReader reader, W3dParseContext context, W3dChunkType chunkType)
        {
            var result = ParseList(reader, context, r => r.ReadColorRgba());
            result._chunkType = chunkType;
            return result;
        }

        protected override void WriteItem(BinaryWriter writer, in ColorRgba item)
        {
            writer.Write(item);
        }
    }
}
