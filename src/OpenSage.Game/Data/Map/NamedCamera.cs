using System;
using System.IO;
using System.Numerics;
using OpenSage.FileFormats;
using OpenSage.Mathematics;

namespace OpenSage.Data.Map
{
    public sealed class NamedCamera
    {
        public Vector3 LookAtPoint { get; private set; }
        public string Name { get; private set; }

        public Vector3 EulerAngles { get; private set; }
        public float Zoom { get; private set; }
        public Vector2 Unknown { get; private set; }

        internal static NamedCamera Parse(BinaryReader reader)
        {
            return new NamedCamera
            {
                LookAtPoint = reader.ReadVector3(),
                Name = reader.ReadUInt16PrefixedAsciiString(),
                EulerAngles = reader.ReadVector3(),
                Zoom = reader.ReadSingle(),
                Unknown = reader.ReadVector2()
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(LookAtPoint);
            writer.WriteUInt16PrefixedAsciiString(Name);
            writer.Write(EulerAngles);
            writer.Write(Zoom);
            writer.Write(Unknown);
        }
    }
}
