using System.Collections.Generic;
using System.IO;

namespace OpenSage.FileFormats.W3d
{

    public sealed class W3dSoundRObj : W3dContainerChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_SOUNDROBJ;

        public W3dSoundRObjHeader Header { get; private set; }

        public W3dSoundRObjDefinition Definition { get; private set; }

        internal static W3dSoundRObj Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dSoundRObj();

                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_SOUNDROBJ_HEADER:
                            result.Header = W3dSoundRObjHeader.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_SOUNDROBJ_DEFINITION:
                            result.Definition = W3dSoundRObjDefinition.Parse(reader, context);
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
            yield return Header;

            if (Definition != null)
            {
                yield return Definition;
            }
        }
    }
}
