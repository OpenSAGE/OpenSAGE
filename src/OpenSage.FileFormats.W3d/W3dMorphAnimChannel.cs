using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d
{
    public sealed class W3dMorphAnimChannel : W3dContainerChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_MORPHANIM_CHANNEL;

        public W3dMorphAnimPoseName PoseName { get; private set; }

        public W3dMorphAnimKeyData KeyData { get; private set; }

        internal static W3dMorphAnimChannel Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dMorphAnimChannel();

                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_MORPHANIM_POSENAME:
                            result.PoseName = W3dMorphAnimPoseName.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_MORPHANIM_KEYDATA:
                            result.KeyData = W3dMorphAnimKeyData.Parse(reader, context);
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
            yield return PoseName;

            if (KeyData != null)
            {
                yield return KeyData;
            }
        }
    }
}
