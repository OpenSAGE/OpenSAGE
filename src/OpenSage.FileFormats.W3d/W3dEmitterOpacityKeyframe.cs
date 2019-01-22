using System.IO;

namespace OpenSage.FileFormats.W3d
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

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(Time);
            writer.Write(Opacity);
        }
    }
}
