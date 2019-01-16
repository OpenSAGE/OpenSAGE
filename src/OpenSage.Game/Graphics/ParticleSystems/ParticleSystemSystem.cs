using System.Collections.Generic;
using OpenSage.Graphics.Rendering;

namespace OpenSage.Graphics.ParticleSystems
{
    internal sealed class ParticleSystemManager : DisposableBase
    {
        private readonly Scene3D _scene;
        private readonly List<AttachedParticleSystem> _deadParticleSystems;

        public ParticleSystemManager(Scene3D scene)
        {
            _scene = scene;

            _deadParticleSystems = new List<AttachedParticleSystem>();
        }

        public void BuildRenderList(RenderList renderList, in GameTime gameTime)
        {
            // TODO: This could be more efficient if we knew upfront about all particle systems.
            // TODO: Keep particle count under GameData.MaxParticleCount
            foreach (var attachedParticleSystem in _scene.GetAllAttachedParticleSystems())
            {
                var particleSystem = attachedParticleSystem.ParticleSystem;

                particleSystem.BuildRenderList(renderList, gameTime);

                if (particleSystem.State == ParticleSystemState.Dead)
                {
                    _deadParticleSystems.Add(attachedParticleSystem);
                }
            }

            foreach (var deadParticleSystem in _deadParticleSystems)
            {
                deadParticleSystem.Detach();
            }

            _deadParticleSystems.Clear();
        }
    }
}
