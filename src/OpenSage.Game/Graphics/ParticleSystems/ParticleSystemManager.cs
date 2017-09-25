using System.Collections.Generic;
using System.Numerics;
using LLGfx;
using OpenSage.Graphics.Effects;

namespace OpenSage.Graphics.ParticleSystems
{
    public sealed class ParticleSystemManager : GraphicsObject
    {
        private readonly List<ParticleSystem> _particleSystems;
        private readonly ParticleEffect _particleEffect;

        public ParticleSystemManager(GraphicsDevice graphicsDevice)
        {
            _particleSystems = new List<ParticleSystem>();

            _particleEffect = AddDisposable(new ParticleEffect(graphicsDevice));
        }

        public void Add(ParticleSystem particleSystem)
        {
            _particleSystems.Add(particleSystem);
        }

        public void Remove(ParticleSystem particleSystem)
        {
            _particleSystems.Remove(particleSystem);
        }

        public void Update(GameTime gameTime)
        {
            // TODO: Don't allocate this every frame.
            var deadParticleSystems = new List<int>();

            for (var i = 0; i < _particleSystems.Count; i++)
            {
                var particleSystem = _particleSystems[i];

                particleSystem.Update(gameTime);

                if (particleSystem.State == ParticleSystemState.Dead)
                {
                    deadParticleSystems.Add(i);
                }
            }

            foreach (var deadParticleSystem in deadParticleSystems)
            {
                _particleSystems.RemoveAt(deadParticleSystem);
            }
        }

        public void Draw(
            CommandEncoder commandEncoder,
            Camera camera)
        {
            _particleEffect.Begin(commandEncoder);

            var world = Matrix4x4.Identity; // TODO

            foreach (var particleSystem in _particleSystems)
            {
                particleSystem.Draw(
                    commandEncoder,
                    _particleEffect,
                    camera,
                    ref world);
            }
        }
    }
}
