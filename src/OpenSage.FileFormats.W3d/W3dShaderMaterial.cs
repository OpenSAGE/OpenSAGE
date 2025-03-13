using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dShaderMaterial(W3dShaderMaterialHeader Header, List<W3dShaderMaterialProperty> Properties)
    : W3dContainerChunk(W3dChunkType.W3D_CHUNK_SHADER_MATERIAL)
{
    internal static W3dShaderMaterial Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            W3dShaderMaterialHeader? resultHeader = null;
            List<W3dShaderMaterialProperty> properties = [];

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_SHADER_MATERIAL_HEADER:
                        resultHeader = W3dShaderMaterialHeader.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_SHADER_MATERIAL_PROPERTY:
                        properties.Add(W3dShaderMaterialProperty.Parse(reader, context));
                        break;

                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            if (resultHeader is null)
            {
                throw new InvalidDataException("header should never be null");
            }

            return new W3dShaderMaterial(resultHeader, properties);
        });
    }

    protected override IEnumerable<W3dChunk> GetSubChunksOverride()
    {
        yield return Header;

        foreach (var property in Properties)
        {
            yield return property;
        }
    }
}
