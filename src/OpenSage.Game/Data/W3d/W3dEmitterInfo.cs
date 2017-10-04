using System.IO;
using System.Numerics;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.W3d
{
    public sealed class W3dEmitterInfo
    {
        public string TextureFileName { get; private set; }

        public float StartSize { get; private set; }
        public float EndSize { get; private set; }
        public float Lifetime { get; private set; }
        public float EmissionRate { get; private set; }
        public float MaxEmissions { get; private set; }
        public float VelocityRandom { get; private set; }
        public float PositionRandom { get; private set; }
        public float FadeTime { get; private set; }
        public float Gravity { get; private set; }
        public float Elasticity { get; private set; }

        public Vector3 Velocity { get; private set; }
        public Vector3 Acceleration { get; private set; }

        public W3dRgba StartColor { get; private set; }
        public W3dRgba EndColor { get; private set; }

        public static W3dEmitterInfo Parse(BinaryReader reader)
        {
            return new W3dEmitterInfo
            {
                TextureFileName = reader.ReadFixedLengthString(260),
                StartSize = reader.ReadSingle(),
                EndSize = reader.ReadSingle(),
                Lifetime = reader.ReadSingle(),
                EmissionRate = reader.ReadSingle(),
                MaxEmissions = reader.ReadSingle(),
                VelocityRandom = reader.ReadSingle(),
                PositionRandom = reader.ReadSingle(),
                FadeTime = reader.ReadSingle(),
                Gravity = reader.ReadSingle(),
                Elasticity = reader.ReadSingle(),

                Velocity = reader.ReadVector3(),
                Acceleration = reader.ReadVector3(),

                StartColor = W3dRgba.Parse(reader),
                EndColor = W3dRgba.Parse(reader)
            };
        }
    }
}
