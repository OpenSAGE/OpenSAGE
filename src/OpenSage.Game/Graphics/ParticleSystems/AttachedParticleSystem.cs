using System;

namespace OpenSage.Graphics.ParticleSystems
{
    public sealed class AttachedParticleSystem
    {
        private readonly Action<AttachedParticleSystem> _detach;

        public ParticleSystem ParticleSystem { get; }

        public AttachedParticleSystem(ParticleSystem particleSystem, Action<AttachedParticleSystem> detach)
        {
            ParticleSystem = particleSystem;
            _detach = detach;
        }

        public void Detach() => _detach(this);
    }
}
