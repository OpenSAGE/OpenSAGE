using System.IO;
using System.Numerics;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    public sealed class CastlePerimeterPoint
    {
        public Vector3 Position { get; private set; }

        internal static CastlePerimeterPoint Parse(BinaryReader reader, ushort version)
        {
            float x, y, z;
            if (version >= 3)
            {
                x = reader.ReadSingle();
                y = reader.ReadSingle();
                z = 0.0f;
            }
            else
            {
                x = reader.ReadInt32();
                y = reader.ReadInt32();
                z = reader.ReadInt32();
            }

            return new CastlePerimeterPoint
            {
                Position = new Vector3(x, y, z)
            };
        }

        internal void WriteTo(BinaryWriter writer, ushort version)
        {
            if (version >= 3)
            {
                writer.Write(Position.X);
                writer.Write(Position.Y);
            }
            else
            {
                writer.Write((int) Position.X);
                writer.Write((int) Position.Y);
                writer.Write((int) Position.Z);
            }
        }
    }
}
