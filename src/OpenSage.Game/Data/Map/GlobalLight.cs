using System.IO;

namespace OpenSage.Data.Map
{
    public sealed class GlobalLight
    {
        public MapVector3 Ambient { get; private set; }
        public MapVector3 Color { get; private set; }
        public MapVector3 EulerAngles { get; private set; }

        internal static GlobalLight Parse(BinaryReader reader)
        {
            return new GlobalLight
            {
                Ambient = MapVector3.Parse(reader),
                Color = MapVector3.Parse(reader),
                EulerAngles = MapVector3.Parse(reader)
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            Ambient.WriteTo(writer);
            Color.WriteTo(writer);
            EulerAngles.WriteTo(writer);
        }
    }
}
