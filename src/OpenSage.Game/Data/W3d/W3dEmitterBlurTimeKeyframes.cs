using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterBlurTimeKeyframes
    {
        public W3dEmitterBlurTimeHeader Header { get; private set; }

        public W3dEmitterBlurTimeKeyframe[] Keyframes { get; private set; }

        internal static W3dEmitterBlurTimeKeyframes Parse(BinaryReader reader)
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
        }
    }
}
