using System;
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
        public readonly ObjectCreationListManager ObjectCreationLists;
        public readonly Terrain.Terrain Terrain;
        public readonly Navigation.Navigation Navigation;
        public readonly Radar Radar;

        public readonly Random Random = new Random();

        // TODO: Make this readonly.
        public GameObjectCollection GameObjects;

        public GameContext(
            AssetLoadContext assetLoadContext,
            AudioSystem audioSystem,
            ParticleSystemManager particleSystems,
            ObjectCreationListManager objectCreationLists,
            Terrain.Terrain terrain,
            Navigation.Navigation navigation,
            Radar radar)
        {
            AssetLoadContext = assetLoadContext;
            AudioSystem = audioSystem;
            ParticleSystems = particleSystems;
            ObjectCreationLists = objectCreationLists;
            Terrain = terrain;
            Navigation = navigation;
            Radar = radar;
        }
    }
}
