using OpenSage.Audio;
using OpenSage.Content.Loaders;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Logic.Object;

namespace OpenSage
{
    internal sealed class GameContext
    {
        public readonly AssetLoadContext AssetLoadContext;
        public readonly AudioSystem AudioSystem;
        public readonly ParticleSystemManager ParticleSystems;
        public readonly Terrain.Terrain Terrain;
        public readonly Navigation.Navigation Navigation;

        // TODO: Make this readonly.
        public GameObjectCollection GameObjects;

        public GameContext(
            AssetLoadContext assetLoadContext,
            AudioSystem audioSystem,
            ParticleSystemManager particleSystems,
            Terrain.Terrain terrain,
            Navigation.Navigation navigation)
        {
            AssetLoadContext = assetLoadContext;
            AudioSystem = audioSystem;
            ParticleSystems = particleSystems;
            Terrain = terrain;
            Navigation = navigation;
        }
    }
}
