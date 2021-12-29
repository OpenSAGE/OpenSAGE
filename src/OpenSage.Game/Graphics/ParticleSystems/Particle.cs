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

        public float AngleX;
        public float AngleY;
        public float AngleZ;

        public float AngularRateX;
        public float AngularRateY;
        public float AngularRateZ;

        public float AngularDamping;

        public int Lifetime;
        public bool Dead;

        public float ColorScale;
        public Vector3 Color;

        public List<ParticleAlphaKeyframe> AlphaKeyframes;
        public float Alpha;

        public List<ParticleColorKeyframe> ColorKeyframes;

        public bool IsParticleUpTowardsEmitter;
        public float UnknownFloat;
        public uint ParticleId;
        public uint UnknownInt2;
        public uint UnknownInt3;
        public uint UnknownInt4;
        public uint UnknownInt5;
        public Vector3 UnknownVector;
        public uint UnknownInt6;

        public void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);
            reader.ReadVersion(1);

            reader.ReadVector3(ref Velocity);
            reader.ReadVector3(ref Position);
            reader.ReadVector3(ref EmitterPosition);
            reader.ReadSingle(ref VelocityDamping);
            reader.ReadSingle(ref AngleX);
            reader.ReadSingle(ref AngleY);
            reader.ReadSingle(ref AngleZ);
            reader.ReadSingle(ref AngularRateX);
            reader.ReadSingle(ref AngularRateY);
            reader.ReadSingle(ref AngularRateZ);
            reader.ReadInt32(ref Lifetime);
            reader.ReadSingle(ref Size);
            reader.ReadSingle(ref SizeRate);
            reader.ReadSingle(ref SizeRateDamping);

            for (var i = 0; i < 8; i++)
            {
                var alphaKeyframeAlpha = 0.0f;
                reader.ReadSingle(ref alphaKeyframeAlpha);

                var alphaKeyframeTime = reader.ReadUInt32();

                AlphaKeyframes.Add(new ParticleAlphaKeyframe(
                    alphaKeyframeTime,
                    alphaKeyframeAlpha));
            }

            for (var i = 0; i < 8; i++)
            {
                Vector3 colorKeyframeColor = default;
                reader.ReadVector3(ref colorKeyframeColor);
                var colorKeyframeTime = reader.ReadUInt32();
                ColorKeyframes.Add(new ParticleColorKeyframe(
                    colorKeyframeTime,
                    colorKeyframeColor));
            }

            reader.ReadSingle(ref ColorScale);
            reader.ReadBoolean(ref IsParticleUpTowardsEmitter);
            reader.ReadSingle(ref UnknownFloat);

            ParticleId = reader.ReadUInt32();

            reader.SkipUnknownBytes(24);

            UnknownInt2 = reader.ReadUInt32(); // 49
            UnknownInt3 = reader.ReadUInt32(); // 1176

            reader.ReadSingle(ref Alpha);

            UnknownInt4 = reader.ReadUInt32(); // 0
            UnknownInt5 = reader.ReadUInt32(); // 1

            reader.ReadVector3(ref Color);
            reader.ReadVector3(ref UnknownVector);

            UnknownInt6 = reader.ReadUInt32(); // 1

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
