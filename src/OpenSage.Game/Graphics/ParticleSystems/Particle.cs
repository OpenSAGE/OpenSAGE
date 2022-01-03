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

        public void Load(StatePersister reader)
        {
            reader.PersistVersion(1);
            reader.PersistVersion(1);

            reader.PersistVector3(ref Velocity);
            reader.PersistVector3(ref Position);
            reader.PersistVector3(ref EmitterPosition);
            reader.PersistSingle(ref VelocityDamping);
            reader.PersistSingle(ref AngleX);
            reader.PersistSingle(ref AngleY);
            reader.PersistSingle(ref AngleZ);
            reader.PersistSingle(ref AngularRateX);
            reader.PersistSingle(ref AngularRateY);
            reader.PersistSingle(ref AngularRateZ);
            reader.PersistInt32(ref Lifetime);
            reader.PersistSingle(ref Size);
            reader.PersistSingle(ref SizeRate);
            reader.PersistSingle(ref SizeRateDamping);

            for (var i = 0; i < 8; i++)
            {
                var alphaKeyframeAlpha = 0.0f;
                reader.PersistSingle(ref alphaKeyframeAlpha);

                var alphaKeyframeTime = 0u;
                reader.PersistUInt32(ref alphaKeyframeTime);

                AlphaKeyframes.Add(new ParticleAlphaKeyframe(
                    alphaKeyframeTime,
                    alphaKeyframeAlpha));
            }

            for (var i = 0; i < 8; i++)
            {
                Vector3 colorKeyframeColor = default;
                reader.PersistVector3(ref colorKeyframeColor);

                var colorKeyframeTime = 0u;
                reader.PersistUInt32(ref colorKeyframeTime);

                ColorKeyframes.Add(new ParticleColorKeyframe(
                    colorKeyframeTime,
                    colorKeyframeColor));
            }

            reader.PersistSingle(ref ColorScale);
            reader.PersistBoolean("IsParticleUpTowardsEmitter", ref IsParticleUpTowardsEmitter);
            reader.PersistSingle(ref UnknownFloat);
            reader.PersistUInt32(ref ParticleId);

            reader.SkipUnknownBytes(24);

            reader.PersistUInt32(ref UnknownInt2); // 49
            reader.PersistUInt32(ref UnknownInt3); // 1176
            reader.PersistSingle(ref Alpha);
            reader.PersistUInt32(ref UnknownInt4); // 0
            reader.PersistUInt32(ref UnknownInt5); // 1
            reader.PersistVector3(ref Color);
            reader.PersistVector3(ref UnknownVector);
            reader.PersistUInt32(ref UnknownInt6); // 1

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
