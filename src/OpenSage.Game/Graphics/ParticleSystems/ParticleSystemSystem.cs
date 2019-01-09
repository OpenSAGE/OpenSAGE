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

        public void Update(GameTime gameTime)
        {
            // TODO: This could be more efficient if we knew upfront about all particle systems.
            foreach (var attachedParticleSystem in _scene.GetAllAttachedParticleSystems())
            {
                var particleSystem = attachedParticleSystem.ParticleSystem;

                particleSystem.Update(gameTime);

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

        public void BuildRenderList(RenderList renderList)
        {
            // TODO: Keep particle count under GameData.MaxParticleCount
            foreach (var attachedParticleSystem in _scene.GetAllAttachedParticleSystems())
            {
                ref readonly var worldMatrix = ref attachedParticleSystem.ParticleSystem.GetWorldMatrix();

                attachedParticleSystem.ParticleSystem.BuildRenderList(
                    renderList,
                    worldMatrix);
            }
        }
    }
}
