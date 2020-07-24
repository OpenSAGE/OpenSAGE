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

        internal static NamedCamera Parse(BinaryReader reader)
        {
            var lookAt = reader.ReadVector3();
            var name = reader.ReadUInt16PrefixedAsciiString();
            //var rotmat = reader.ReadMatrix3x2();

            var eulerAngles = reader.ReadVector3();

            var zoom = reader.ReadSingle();
            var unknown2 = reader.ReadSingle();
            var unknown3 = reader.ReadSingle();

            return new NamedCamera
            {
                LookAtPoint = lookAt,
                Name = name,
                EulerAngles = eulerAngles,
                Zoom = zoom
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(LookAtPoint);
            writer.WriteUInt16PrefixedAsciiString(Name);
            writer.Write(EulerAngles);
            writer.Write(Zoom);
            writer.Write(Vector2.Zero);
        }
    }
}
