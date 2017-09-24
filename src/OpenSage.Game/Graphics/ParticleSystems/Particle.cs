using System.Numerics;

namespace OpenSage.Graphics.ParticleSystems
{
    internal struct Particle
    {
        public int Timer;

        public Vector3 Position;

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
    }
}
