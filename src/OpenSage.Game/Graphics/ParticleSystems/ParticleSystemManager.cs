using System.Collections.Generic;
using OpenSage.Content.Loaders;
using OpenSage.Graphics.Rendering;

namespace OpenSage.Graphics.ParticleSystems
{
    internal sealed class ParticleSystemManager : DisposableBase
    {
        private readonly AssetLoadContext _loadContext;
        private readonly int _maxParticleCount;

        private readonly List<ParticleSystem> _particleSystems;

        public ParticleSystemManager(AssetLoadContext assetLoadContext)
        {
            _loadContext = assetLoadContext;
            _maxParticleCount = assetLoadContext.AssetStore.GameData.Current.MaxParticleCount;

            _particleSystems = new List<ParticleSystem>();
        }

        public ParticleSystem Create(
            FXParticleSystemTemplate template,
            ParticleSystem.GetMatrixReferenceDelegate getWorldMatrix)
        {
            ParticleSystem result;

            _particleSystems.Add(
                AddDisposable(
                    result = new ParticleSystem(
                        template,
                        _loadContext,
                        getWorldMatrix)));

            return result;
        }

        public void Remove(ParticleSystem particleSystem)
        {
            if (_particleSystems.Remove(particleSystem))
            {
                RemoveAndDispose(ref particleSystem);
            }
        }

        public void BuildRenderList(RenderList renderList)
        {
            // TODO: Sort by ParticleSystem.Priority.

            var totalParticles = 0;

            for (var i = 0; i < _particleSystems.Count; i++)
            {
                var particleSystem = _particleSystems[i];

                if (particleSystem.State == ParticleSystemState.Inactive)
                {
                    continue;
                }

                particleSystem.BuildRenderList(renderList);

                if (particleSystem.State == ParticleSystemState.Dead)
                {
                    RemoveToDispose(particleSystem);
                    _particleSystems.RemoveAt(i);
                    i--;
                }

                totalParticles += particleSystem.CurrentParticleCount;

                if (totalParticles > _maxParticleCount)
                {
                    break;
                }
            }
        }
    }
}
