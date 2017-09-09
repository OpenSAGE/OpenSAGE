using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterOpacityKeyframe
    {
        public float Time { get; private set; }
        public float Opacity { get; private set; }

        internal static W3dEmitterOpacityKeyframe Parse(BinaryReader reader)
        {
            return new W3dEmitterOpacityKeyframe
            {
                Time = reader.ReadSingle(),
                Opacity = reader.ReadSingle()
            };
        }
    }
}
