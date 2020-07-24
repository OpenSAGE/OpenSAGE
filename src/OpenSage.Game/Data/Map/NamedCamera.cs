using System;
using System.IO;
using System.Numerics;
using OpenSage.FileFormats;
using OpenSage.Mathematics;

namespace OpenSage.Data.Map
{
    public sealed class NamedCamera
    {
        public Vector2 LookAtPoint { get; private set; }
        public string Name { get; private set; }

        public float Pitch { get; private set; }
        public float Roll { get; private set; }
        public float Yaw { get; private set; }
        public float Zoom { get; private set; }

        internal static NamedCamera Parse(BinaryReader reader)
        {
            var lookAt = reader.ReadVector2();
            var unknown1 = reader.ReadSingle();
            var name = reader.ReadUInt16PrefixedAsciiString();
            //var rotmat = reader.ReadMatrix3x2();

            var pitch = MathUtility.ToDegrees(reader.ReadSingle());
            var roll = MathUtility.ToDegrees(reader.ReadSingle());
            var yaw = MathUtility.ToDegrees(reader.ReadSingle());

            var zoom = reader.ReadSingle();
            var unknown2 = reader.ReadSingle();
            var unknown3 = reader.ReadSingle();

            return new NamedCamera
            {
                LookAtPoint = lookAt,
                Name = name,
                Pitch = pitch,
                Roll = roll,
                Yaw = yaw,
                Zoom = zoom
            };
        }

        internal void WriteTo(BinaryWriter writer)
        {
            writer.Write(LookAtPoint);
            writer.Write(0.0f);
            writer.WriteUInt16PrefixedAsciiString(Name);

            writer.Write(MathUtility.ToRadians(Pitch));
            writer.Write(MathUtility.ToRadians(Roll));
            writer.Write(MathUtility.ToRadians(Yaw)); ;
            writer.Write(Zoom);
            writer.Write(Vector2.Zero);
        }
    }
}
