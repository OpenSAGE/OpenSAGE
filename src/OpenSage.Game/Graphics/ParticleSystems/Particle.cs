using System.Numerics;

namespace OpenSage.Graphics.ParticleSystems
{
    internal struct Particle : IPersistableObject
    {
        private readonly ParticleSystem _system;

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

        public readonly ParticleAlphaKeyframe[] AlphaKeyframes;
        public float Alpha;

        public bool IsParticleUpTowardsEmitter;
        public float UnknownFloat;
        public uint ParticleId;
        public uint UnknownInt2;
        public uint UnknownInt3;
        public uint UnknownInt4;
        public uint UnknownInt5;
        public Vector3 UnknownVector;
        public uint UnknownInt6;

        public Particle(ParticleSystem system)
        {
            _system = system;

            Timer = 0;
            Position = Vector3.Zero;
            EmitterPosition = Vector3.Zero;
            Velocity = Vector3.Zero;
            VelocityDamping = 0;
            Size = 0;
            SizeRate = 0;
            SizeRateDamping = 0;
            AngleZ = 0;
            AngularRateZ = 0;
            AngularDamping = 0;
            Lifetime = 0;
            Dead = true;
            ColorScale = 0;
            Color = Vector3.Zero;
            AlphaKeyframes = new ParticleAlphaKeyframe[8];
            Alpha = 0;
            IsParticleUpTowardsEmitter = false;
            UnknownFloat = 0;
            ParticleId = 0;
            UnknownInt2 = 0;
            UnknownInt3 = 0;
            UnknownInt4 = 0;
            UnknownInt5 = 0;
            UnknownVector = Vector3.Zero;
            UnknownInt6 = 0;
        }

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);
            reader.PersistVersion(1);

            var unusedFloat = 0.0f;

            reader.PersistVector3("Velocity", ref Velocity);
            reader.PersistVector3("Position", ref Position);
            reader.PersistVector3("EmitterPosition", ref EmitterPosition);
            reader.PersistSingle("VelocityDamping", ref VelocityDamping);
            reader.PersistSingle("AngleX", ref unusedFloat);
            reader.PersistSingle("AngleY", ref unusedFloat);
            reader.PersistSingle("AngleZ", ref AngleZ);
            reader.PersistSingle("AngularRateX", ref unusedFloat);
            reader.PersistSingle("AngularRateY", ref unusedFloat);
            reader.PersistSingle("AngularRateZ", ref AngularRateZ);
            reader.PersistInt32("Lifetime", ref Lifetime);
            reader.PersistSingle("Size", ref Size);
            reader.PersistSingle("SizeRate", ref SizeRate);
            reader.PersistSingle("SizeRateDamping", ref SizeRateDamping);

            reader.PersistArray("AlphaKeyframes", AlphaKeyframes, static (StatePersister persister, ref ParticleAlphaKeyframe item) =>
            {
                persister.PersistObjectValue(ref item);
            });

            reader.PersistArray("ColorKeyframes", _system.ColorKeyframes, static (StatePersister persister, ref ParticleColorKeyframe item) =>
            {
                persister.PersistObjectValue(ref item);
            });

            reader.PersistSingle("ColorScale", ref ColorScale);
            reader.PersistBoolean("IsParticleUpTowardsEmitter", ref IsParticleUpTowardsEmitter);
            reader.PersistSingle("UnknownFloat", ref UnknownFloat);
            reader.PersistUInt32("ParticleId", ref ParticleId);

            reader.SkipUnknownBytes(24);

            reader.PersistUInt32("UnknownInt2", ref UnknownInt2); // 49
            reader.PersistUInt32("UnknownInt3", ref UnknownInt3); // 1176
            reader.PersistSingle("Alpha", ref Alpha);
            reader.PersistUInt32("UnknownInt4", ref UnknownInt4); // 0
            reader.PersistUInt32("UnknownInt5", ref UnknownInt5); // 1
            reader.PersistVector3("Color", ref Color);
            reader.PersistVector3("UnknownVector", ref UnknownVector);
            reader.PersistUInt32("UnknownInt6", ref UnknownInt6); // 1

            reader.SkipUnknownBytes(8);
        }
    }

    internal struct ParticleAlphaKeyframe : IParticleKeyframe, IPersistableObject
    {
        public uint Time;
        public float Alpha;

        uint IParticleKeyframe.Time => Time;

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

        public void Persist(StatePersister persister)
        {
            persister.PersistSingle("Alpha", ref Alpha);
            persister.PersistUInt32("Time", ref Time);
        }
    }
}
