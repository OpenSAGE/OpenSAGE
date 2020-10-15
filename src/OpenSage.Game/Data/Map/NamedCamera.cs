using System.IO;
using System.Numerics;
using OpenSage.FileFormats;

namespace OpenSage.Data.Map
{
    public sealed class NamedCamera
    {
        public Vector3 LookAtPoint { get; private set; }
        public string Name { get; private set; }

        public float Pitch { get; private set; }
        public float Roll { get; private set; }
        public float Yaw { get; private set; }
        public float Zoom { get; private set; }
        public float FieldOfView { get; private set; }
        public float Unknown { get; private set; }

        internal static NamedCamera Parse(BinaryReader reader)
        {
            return new NamedCamera
            {
                LookAtPoint = reader.ReadVector3(),
                Name = reader.ReadUInt16PrefixedAsciiString(),
                Pitch = reader.ReadSingle(),
                Roll = reader.ReadSingle(),
                Yaw = reader.ReadSingle(),
                Zoom = reader.ReadSingle(),
                FieldOfView = reader.ReadSingle(),
                Unknown = reader.ReadSingle()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(LookAtPoint);
            writer.WriteUInt16PrefixedAsciiString(Name);
            writer.Write(Pitch);
            writer.Write(Roll);
            writer.Write(Yaw);
            writer.Write(Zoom);
            writer.Write(FieldOfView);
            writer.Write(Unknown);
        }
    }
}
