using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Map
{
    public sealed class GlobalLight
    {
        public Vector3 Ambient { get; private set; }
        public Vector3 Color { get; private set; }
        public Vector3 Direction { get; private set; }

        internal static GlobalLight Parse(BinaryReader reader)
        {
            return new GlobalLight
            {
                Ambient = reader.ReadVector3(),
                Color = reader.ReadVector3(),
                Direction = reader.ReadVector3()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(Ambient);
            writer.Write(Color);
            writer.Write(Direction);
        }
    }
}
