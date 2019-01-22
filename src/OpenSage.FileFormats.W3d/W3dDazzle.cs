using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.W3d
{
    /********************************************************************************

        Dazzle Objects

        The only data needed to instantiate a dazzle object is the type-name of
        the dazzle to use. The dazzle is always assumed to be at the pivot point
        of the bone it is attached to (you should enable Export_Transform) for 
        dazzles. If the dazzle-type (from dazzle.ini) is directional, then the 
        coordinate-system of the bone will define the direction.

    ********************************************************************************/

    public sealed class W3dDazzle : W3dContainerChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_DAZZLE;

        public W3dDazzleName Name { get; private set; }

        public W3dDazzleTypeName TypeName { get; private set; }

        internal static W3dDazzle Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dDazzle();

                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_DAZZLE_NAME:
                            result.Name = W3dDazzleName.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_DAZZLE_TYPENAME:
                            result.TypeName = W3dDazzleTypeName.Parse(reader, context);
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
            yield return Name;

            if (TypeName != null)
            {
                yield return TypeName;
            }
        }
    }
}
