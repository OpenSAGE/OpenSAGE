using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

/********************************************************************************

    Dazzle Objects

    The only data needed to instantiate a dazzle object is the type-name of
    the dazzle to use. The dazzle is always assumed to be at the pivot point
    of the bone it is attached to (you should enable Export_Transform) for
    dazzles. If the dazzle-type (from dazzle.ini) is directional, then the
    coordinate-system of the bone will define the direction.

********************************************************************************/

public sealed record W3dDazzle(W3dDazzleName Name, W3dDazzleTypeName? TypeName) : W3dContainerChunk(W3dChunkType.W3D_CHUNK_DAZZLE)
{
    internal static W3dDazzle Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            W3dDazzleName? name = null;
            W3dDazzleTypeName? typeName = null;

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_DAZZLE_NAME:
                        name = W3dDazzleName.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_DAZZLE_TYPENAME:
                        typeName = W3dDazzleTypeName.Parse(reader, context);
                        break;

                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            if (name is null)
            {
                throw new InvalidDataException("name should never be null");
            }

            return new W3dDazzle(name, typeName);
        });
    }

    protected override IEnumerable<W3dChunk> GetSubChunksOverride()
    {
        yield return Name;

        if (TypeName != null)
        {
            yield return TypeName;
        }
    }
}
