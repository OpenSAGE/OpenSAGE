using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterFrameKeyframes : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_EMITTER_FRAME_KEYFRAMES;

        public W3dEmitterFrameHeader Header { get; private set; }

        public W3dEmitterFrameKeyframe[] Keyframes { get; private set; }

        internal static W3dEmitterFrameKeyframes Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dEmitterFrameKeyframes
                {
                    Header = W3dEmitterFrameHeader.Parse(reader)
                };

                // Certain enb w3d's break without this check. (example: astro00.w3d)
                if (reader.BaseStream.Position < context.CurrentEndPosition)
                {
                    var remaining = (int) context.CurrentEndPosition - (int) reader.BaseStream.Position;
                    var KeyframeCount = remaining / 8;
                    result.Keyframes = new W3dEmitterFrameKeyframe[KeyframeCount];
                    for (var i = 0; i < result.Keyframes.Length; i++)
                    {
                        result.Keyframes[i] = W3dEmitterFrameKeyframe.Parse(reader);
                    }
                }

                return result;
            });
        }

        protected override void WriteToOverride(BinaryWriter writer)
        {
            Header.WriteTo(writer);

            foreach (var keyframe in Keyframes)
            {
                keyframe.WriteTo(writer);
            }
        }
    }
}
