using System;

namespace OpenSage.Graphics.ParticleSystems
{
    public sealed class AttachedParticleSystem
    {
        private readonly Action<AttachedParticleSystem> _detach;

        public readonly ParticleSystem ParticleSystem;

        internal AttachedParticleSystem(ParticleSystem particleSystem, Action<AttachedParticleSystem> detach)
        {
            ParticleSystem = particleSystem;
            _detach = detach;
        }

        public void Detach() => _detach(this);
    }
}
