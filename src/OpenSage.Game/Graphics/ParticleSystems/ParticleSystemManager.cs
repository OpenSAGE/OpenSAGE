using System.Collections.Generic;
using OpenSage.Graphics.Rendering;

namespace OpenSage.Graphics.ParticleSystems
{
    public sealed class ParticleSystemSystem : GameSystem
    {
        private readonly List<ParticleSystem> _particleSystems;

        private readonly List<ParticleSystem> _deadParticleSystems;

        public ParticleSystemSystem(Game game)
            : base(game)
        {
            RegisterComponentList(_particleSystems = new List<ParticleSystem>());

            _deadParticleSystems = new List<ParticleSystem>();

            game.ContentManager.IniDataContext.LoadIniFile(@"Data\INI\ParticleSystem.ini");
        }

        public override void Update(GameTime gameTime)
        {
            for (var i = 0; i < _particleSystems.Count; i++)
            {
                var particleSystem = _particleSystems[i];

                particleSystem.Update(gameTime);

                if (particleSystem.State == ParticleSystemState.Dead)
                {
                    _deadParticleSystems.Add(particleSystem);
                }
            }

            foreach (var deadParticleSystem in _deadParticleSystems)
            {
                deadParticleSystem.Entity.Components.Remove(deadParticleSystem);
            }

            _deadParticleSystems.Clear();
        }

        internal override void BuildRenderList(RenderList renderList)
        {
            foreach (var particleSystem in _particleSystems)
            {
                particleSystem.BuildRenderList(renderList);
            }
        }
    }
}
