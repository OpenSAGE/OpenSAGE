using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content.Loaders;
using OpenSage.Graphics.Rendering;

namespace OpenSage.Graphics.ParticleSystems
{
    internal sealed class ParticleSystemManager : DisposableBase, IPersistableObject
    {
        private readonly AssetLoadContext _loadContext;
        private readonly int _maxParticleCount;

        private readonly List<ParticleSystem> _particleSystems;

        private uint _previousParticleSystemId;

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

        public ParticleSystem Create(
            FXParticleSystemTemplate template,
            in Matrix4x4 worldMatrix)
        {
            ParticleSystem result;

            _particleSystems.Add(
                AddDisposable(
                    result = new ParticleSystem(
                        template,
                        _loadContext,
                        worldMatrix)));

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

        public void Persist(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.PersistUInt32(ref _previousParticleSystemId);

            var count = (uint)_particleSystems.Count;
            reader.PersistUInt32(ref count);

            reader.BeginArray("ParticleSystems");
            if (reader.Mode == StatePersistMode.Read)
            {
                for (var i = 0; i < count; i++)
                {
                    reader.BeginObject();

                    var templateName = "";
                    reader.PersistAsciiString(ref templateName);

                    if (templateName != string.Empty)
                    {
                        var template = _loadContext.AssetStore.FXParticleSystemTemplates.GetByName(templateName);

                        var particleSystem = Create(
                            template,
                            Matrix4x4.Identity); // TODO

                        reader.PersistObject(particleSystem);
                    }

                    reader.EndObject();
                }
            }
            else
            {
                foreach (var particleSystem in _particleSystems)
                {
                    reader.BeginObject();

                    var templateName = particleSystem.Template.Name;
                    reader.PersistAsciiString(ref templateName);

                    reader.PersistObject(particleSystem);

                    reader.EndObject();
                }
            }
            reader.EndArray();
        }
    }
}
