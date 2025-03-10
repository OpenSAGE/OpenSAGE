using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d;

public sealed record W3dMorphAnimChannel(W3dMorphAnimPoseName PoseName, W3dMorphAnimKeyData? KeyData)
    : W3dContainerChunk(W3dChunkType.W3D_CHUNK_MORPHANIM_CHANNEL)
{
    internal static W3dMorphAnimChannel Parse(BinaryReader reader, W3dParseContext context)
    {
        return ParseChunk(reader, context, header =>
        {
            W3dMorphAnimPoseName? poseName = null;
            W3dMorphAnimKeyData? keyData = null;

            ParseChunks(reader, context.CurrentEndPosition, chunkType =>
            {
                switch (chunkType)
                {
                    case W3dChunkType.W3D_CHUNK_MORPHANIM_POSENAME:
                        poseName = W3dMorphAnimPoseName.Parse(reader, context);
                        break;

                    case W3dChunkType.W3D_CHUNK_MORPHANIM_KEYDATA:
                        keyData = W3dMorphAnimKeyData.Parse(reader, context);
                        break;

                    default:
                        throw CreateUnknownChunkException(chunkType);
                }
            });

            if (poseName is null)
            {
                throw new InvalidDataException("poseName should never be null");
            }

            return new W3dMorphAnimChannel(poseName, keyData);
        });
    }

    protected override IEnumerable<W3dChunk> GetSubChunksOverride()
    {
        yield return PoseName;

        if (KeyData != null)
        {
            yield return KeyData;
        }
    }
}
