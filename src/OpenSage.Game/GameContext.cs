using System;
using OpenSage.Audio;
using OpenSage.Client;
using OpenSage.Content.Loaders;
using OpenSage.DataStructures;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Logic;
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

        public Game Game => Scene3D.Game;

        public GameLogic GameLogic => Scene3D.Game.GameLogic;

        public GameClient GameClient => Scene3D.Game.GameClient;

        public readonly Random Random = new Random();

        public LogicFrameSpan GetRandomLogicFrameSpan(LogicFrameSpan min, LogicFrameSpan max)
        {
            var value = Random.Next(
                (int)min.Value,
                (int)max.Value);
            return new LogicFrameSpan((uint)value);
        }

        // TODO: Make this readonly.
        public GameObjectCollection GameObjects;
        public readonly Quadtree<GameObject> Quadtree;

        // TODO: This is temporary until Scene3D and GameContext are merged.
        public readonly Scene3D Scene3D;

        public GameContext(
            AssetLoadContext assetLoadContext,
            AudioSystem audioSystem,
            ParticleSystemManager particleSystems,
            ObjectCreationListManager objectCreationLists,
            Terrain.Terrain terrain,
            Navigation.Navigation navigation,
            Radar radar,
            Quadtree<GameObject> quadtree,
            Scene3D scene)
        {
            AssetLoadContext = assetLoadContext;
            AudioSystem = audioSystem;
            ParticleSystems = particleSystems;
            ObjectCreationLists = objectCreationLists;
            Terrain = terrain;
            Navigation = navigation;
            Radar = radar;
            Scene3D = scene;
            Quadtree = quadtree;
        }
    }
}
