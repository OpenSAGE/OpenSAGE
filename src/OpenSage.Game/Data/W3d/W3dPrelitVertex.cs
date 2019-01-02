using System.Collections.Generic;
using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dPrelitVertex : W3dContainerChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_PRELIT_VERTEX;

        public W3dMaterialInfo MaterialInfo { get; private set; }

        public W3dVertexMaterials VertexMaterials { get; private set; }

        public W3dShaders Shaders { get; private set; }

        public W3dTextures Textures { get; private set; }

        public W3dMaterialPass MaterialPass { get; private set; }

        internal static W3dPrelitVertex Parse(BinaryReader reader, W3dParseContext context)
        {
            var parsedChunk = ParseChunk(reader, context, header =>
            {
                var result = new W3dPrelitVertex();

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
                            result.MaterialPass = W3dMaterialPass.Parse(reader, context);
                            break;

                        default:
                            throw CreateUnknownChunkException(chunkType);
                    }
                });

                return result;
            });

            return parsedChunk;
        }

        protected override IEnumerable<W3dChunk> GetSubChunksOverride()
        {
            yield return MaterialInfo;
        }

    }
}
