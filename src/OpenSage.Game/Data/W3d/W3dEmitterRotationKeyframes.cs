using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterRotationKeyframes
    {
        public W3dEmitterRotationHeader Header { get; private set; }

        public W3dEmitterRotationKeyframe[] Keyframes { get; private set; }

        internal static W3dEmitterRotationKeyframes Parse(BinaryReader reader)
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
        }
    }
}
