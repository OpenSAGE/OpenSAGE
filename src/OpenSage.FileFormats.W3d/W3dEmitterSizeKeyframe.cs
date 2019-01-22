using System.IO;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterSizeKeyframe
    {
        public float Time { get; private set; }
        public float Size { get; private set; }

        internal static W3dEmitterSizeKeyframe Parse(BinaryReader reader)
        {
            return new W3dEmitterSizeKeyframe
            {
                Time = reader.ReadSingle(),
                Size = reader.ReadSingle()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(Time);
            writer.Write(Size);
        }
    }
}
