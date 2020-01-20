using OpenSage.Audio;
using OpenSage.Content.Loaders;
using OpenSage.Graphics.ParticleSystems;

namespace OpenSage
{
    internal sealed class GameContext
    {
        public readonly AssetLoadContext AssetLoadContext;
        public readonly AudioSystem AudioSystem;
        public readonly ParticleSystemManager ParticleSystems;
        public readonly Terrain.Terrain Terrain;

        public GameContext(
            AssetLoadContext assetLoadContext,
            AudioSystem audioSystem,
            ParticleSystemManager particleSystems,
            Terrain.Terrain terrain)
        {
            AssetLoadContext = assetLoadContext;
            AudioSystem = audioSystem;
            ParticleSystems = particleSystems;
            Terrain = terrain;
        }
    }
}
