using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dAnimation : W3dContainerChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_ANIMATION;

        public W3dAnimationHeader Header { get; private set; }

        public List<W3dAnimationChannelBase> Channels { get; } = new List<W3dAnimationChannelBase>();

        internal static W3dAnimation Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dAnimation();

                // TODO: generals zero hour assets: UISabotr throw chunktype errors.
                // Either new chunks were added for zero hour, parsing is incorrect or
                // they are corrupted w3d files. They all throw errors in w3d viewer.
                ParseChunks(reader, context.CurrentEndPosition, chunkType =>
                {
                    switch (chunkType)
                    {
                        case W3dChunkType.W3D_CHUNK_ANIMATION_HEADER:
                            result.Header = W3dAnimationHeader.Parse(reader, context);
                            break;

                        case W3dChunkType.W3D_CHUNK_ANIMATION_CHANNEL:
                            result.Channels.Add(W3dAnimationChannel.Parse(reader, context));
                            break;

                        case W3dChunkType.W3D_CHUNK_BIT_CHANNEL:
                            result.Channels.Add(W3dBitChannel.Parse(reader, context));
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

            foreach (var channel in Channels)
            {
                yield return channel;
            }
        }
    }
}
