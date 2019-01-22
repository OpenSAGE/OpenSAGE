using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public abstract class W3dPrelitBase<TDerived> : W3dContainerChunk
        where TDerived : W3dPrelitBase<TDerived>, new()
    {
        public W3dMaterialInfo MaterialInfo { get; private set; }

        public W3dVertexMaterials VertexMaterials { get; private set; }

        public W3dShaders Shaders { get; private set; }

        public W3dTextures Textures { get; private set; }

        public W3dMaterialPass MaterialPass1 { get; private set; }

        public W3dMaterialPass MaterialPass2 { get; private set; }

        internal static TDerived Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new TDerived();

                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_MATERIAL_INFO:
                            result.MaterialInfo = W3dMaterialInfo.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_VERTEX_MATERIALS:
                            result.VertexMaterials = W3dVertexMaterials.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_SHADERS:
                            result.Shaders = W3dShaders.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_TEXTURES:
                            result.Textures = W3dTextures.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_MATERIAL_PASS:
                            if (result.MaterialPass1 == null)
                            {
                                result.MaterialPass1 = W3dMaterialPass.Parse(reader, context);
                            }
                            else
                            {
                                result.MaterialPass2 = W3dMaterialPass.Parse(reader, context);
                            }
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
            yield return MaterialInfo;

            if (VertexMaterials != null)
            {
                yield return VertexMaterials;
            }

            if (Shaders != null)
            {
                yield return Shaders;
            }

            if (Textures != null)
            {
                yield return Textures;
            }

            if (MaterialPass1 != null)
            {
                yield return MaterialPass1;
            }

            if (MaterialPass2 != null)
            {
                yield return MaterialPass2;
            }
        }
    }
}