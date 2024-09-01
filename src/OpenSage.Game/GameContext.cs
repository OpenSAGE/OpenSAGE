using System;
using OpenSage.Audio;
using OpenSage.Client;
using OpenSage.Content;
using OpenSage.Content.Loaders;
using OpenSage.DataStructures;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Logic;
using OpenSage.Logic.Object;

namespace OpenSage
{
    public sealed class GameContext
    {
        internal readonly AssetLoadContext AssetLoadContext;
        public readonly AudioSystem AudioSystem;
        internal readonly ParticleSystemManager ParticleSystems;
        internal readonly ObjectCreationListManager ObjectCreationLists;
        public readonly Terrain.Terrain Terrain;
        public readonly Navigation.Navigation Navigation;
        public readonly Radar Radar;

        public readonly IGame Game;

        internal GameLogic GameLogic => Game.GameLogic;

        internal GameClient GameClient => Game.GameClient;

        public AssetStore AssetStore => AssetLoadContext.AssetStore;

        public readonly Random Random = new Random();

        public LogicFrameSpan GetRandomLogicFrameSpan(LogicFrameSpan min, LogicFrameSpan max)
        {
            var value = Random.Next(
                (int)min.Value,
                (int)max.Value);
            return new LogicFrameSpan((uint)value);
        }

        public readonly IQuadtree<GameObject> Quadtree;

        // TODO: This is temporary until Scene3D and GameContext are merged.
        public readonly Scene3D Scene3D;

        internal GameContext(
            AssetLoadContext assetLoadContext,
            AudioSystem audioSystem,
            ParticleSystemManager particleSystems,
            ObjectCreationListManager objectCreationLists,
            Terrain.Terrain terrain,
            Navigation.Navigation navigation,
            Radar radar,
            IQuadtree<GameObject> quadtree,
            Scene3D scene,
            IGame game)
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
            Game = game;
        }
    }
}
