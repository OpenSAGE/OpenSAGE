using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterBlurTimeKeyframes : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_EMITTER_BLUR_TIME_KEYFRAMES;

        public W3dEmitterBlurTimeHeader Header { get; private set; }

        public W3dEmitterBlurTimeKeyframe[] Keyframes { get; private set; }

        internal static W3dEmitterBlurTimeKeyframes Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dEmitterBlurTimeKeyframes
                {
                    Header = W3dEmitterBlurTimeHeader.Parse(reader)
                };

                result.Keyframes = new W3dEmitterBlurTimeKeyframe[result.Header.KeyframeCount + 1];
                for (var i = 0; i < result.Keyframes.Length; i++)
                {
                    result.Keyframes[i] = W3dEmitterBlurTimeKeyframe.Parse(reader);
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
