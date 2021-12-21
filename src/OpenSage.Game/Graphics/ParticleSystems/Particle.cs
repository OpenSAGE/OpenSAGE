using System.Collections.Generic;
using System.Numerics;

namespace OpenSage.Graphics.ParticleSystems
{
    internal struct Particle
    {
        public int Timer;

        public Vector3 Position;
        public Vector3 EmitterPosition;

        public Vector3 Velocity;
        public float VelocityDamping;

        public float Size;
        public float SizeRate;
        public float SizeRateDamping;

        public float AngleZ;
        public float AngularRateZ;
        public float AngularDamping;

        public int Lifetime;
        public bool Dead;

        public float ColorScale;
        public Vector3 Color;

        public List<ParticleAlphaKeyframe> AlphaKeyframes;
        public float Alpha;

        public void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);
            reader.ReadVersion(1);

            Velocity = reader.ReadVector3();

            Position = reader.ReadVector3();

            EmitterPosition = reader.ReadVector3();

            VelocityDamping = reader.ReadSingle();

            reader.ReadSingle(); // AngleX
            reader.ReadSingle(); // AngleY
            AngleZ = reader.ReadSingle();

            reader.ReadSingle(); // AngularRateX
            reader.ReadSingle(); // AngularRateY
            AngularRateZ = reader.ReadSingle();

            Lifetime = reader.ReadInt32();

            Size = reader.ReadSingle();

            SizeRate = reader.ReadSingle();

            SizeRateDamping = reader.ReadSingle();

            for (var i = 0; i < 8; i++)
            {
                var alphaKeyframeAlpha = reader.ReadSingle();
                var alphaKeyframeTime = reader.ReadUInt32();
                var alphaKeyframe = new ParticleAlphaKeyframe(
                    alphaKeyframeTime,
                    alphaKeyframeAlpha);
            }

            for (var i = 0; i < 8; i++)
            {
                var colorKeyframeColor = reader.ReadVector3();
                var colorKeyframeTime = reader.ReadUInt32();
                var colorKeyframe = new ParticleColorKeyframe(
                    colorKeyframeTime,
                    colorKeyframeColor);
            }

            ColorScale = reader.ReadSingle();

            reader.ReadBoolean(); // IsParticleUpTowardsEmitter

            var unknown1 = reader.ReadSingle();

            reader.ReadUInt32(); // ParticleID

            reader.SkipUnknownBytes(24);

            var unknown2 = reader.ReadUInt32(); // 49
            var unknown3 = reader.ReadUInt32(); // 1176

            Alpha = reader.ReadSingle();

            var unknown4 = reader.ReadUInt32(); // 0
            var unknown5 = reader.ReadUInt32(); // 1

            Color = reader.ReadVector3();

            var unknown6 = reader.ReadVector3();

            var unknown7 = reader.ReadUInt32(); // 1

            reader.SkipUnknownBytes(8);
        }
    }

    internal readonly struct ParticleAlphaKeyframe : IParticleKeyframe
    {
        public uint Time { get; }
        public readonly float Alpha;

        public ParticleAlphaKeyframe(RandomAlphaKeyframe keyframe)
        {
            Time = keyframe.Time;
            Alpha = ParticleSystemUtility.GetRandomFloat(keyframe.Value);
        }

        public ParticleAlphaKeyframe(uint time, float alpha)
        {
            Time = time;
            Alpha = alpha;
        }
    }
}
