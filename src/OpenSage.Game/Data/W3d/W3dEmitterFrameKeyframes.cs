using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterFrameKeyframes
    {
        public W3dEmitterFrameHeader Header { get; private set; }

        public W3dEmitterFrameKeyframe[] Keyframes { get; private set; }

        internal static W3dEmitterFrameKeyframes Parse(BinaryReader reader)
        {
            var result = new W3dEmitterFrameKeyframes
            {
                Header = W3dEmitterFrameHeader.Parse(reader)
            };

            result.Keyframes = new W3dEmitterFrameKeyframe[result.Header.KeyframeCount + 1];
            for (var i = 0; i < result.Keyframes.Length; i++)
            {
                result.Keyframes[i] = W3dEmitterFrameKeyframe.Parse(reader);
            }

            return result;
        }

        internal void WriteTo(BinaryWriter writer)
        {
            Header.WriteTo(writer);

            foreach (var keyframe in Keyframes)
            {
                keyframe.WriteTo(writer);
            }
        }
    }
}
