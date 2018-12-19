using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterRotationKeyframes : W3dChunk
    {
        public override W3dChunkType ChunkType { get; } = W3dChunkType.W3D_CHUNK_EMITTER_ROTATION_KEYFRAMES;

        public W3dEmitterRotationHeader Header { get; private set; }

        public W3dEmitterRotationKeyframe[] Keyframes { get; private set; }

        internal static W3dEmitterRotationKeyframes Parse(BinaryReader reader, W3dParseContext context)
        {
            return ParseChunk(reader, context, header =>
            {
                var result = new W3dEmitterRotationKeyframes
                {
                    Header = W3dEmitterRotationHeader.Parse(reader)
                };

                result.Keyframes = new W3dEmitterRotationKeyframe[result.Header.KeyframeCount + 1];
                for (var i = 0; i < result.Keyframes.Length; i++)
                {
                    result.Keyframes[i] = W3dEmitterRotationKeyframe.Parse(reader);
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
