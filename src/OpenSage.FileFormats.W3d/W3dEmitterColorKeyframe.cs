using System.IO;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.Mathematics;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterColorKeyframe
    {
        public float Time { get; private set; }
        public ColorRgba Color { get; private set; }

        internal static W3dEmitterColorKeyframe Parse(BinaryReader reader)
        {
            return new W3dEmitterColorKeyframe
            {
                Time = reader.ReadSingle(),
                Color = reader.ReadColorRgba()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(Time);
            writer.Write(Color);
        }
    }
}
