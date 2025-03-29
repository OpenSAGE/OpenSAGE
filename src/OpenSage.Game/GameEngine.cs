using System;
using System.Text;
using OpenSage.Audio;
using OpenSage.Client;
using OpenSage.Content;
using OpenSage.Content.Loaders;
using OpenSage.DataStructures;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Logic;
using OpenSage.Logic.AI;
using OpenSage.Logic.Object;

namespace OpenSage;

public sealed class GameEngine
{
    static GameEngine()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <summary>
    /// "Ideal" logic frames per second, used for converting module data and animations.
    /// </summary>
    public float LogicFramesPerSecond { get; }

    /// <summary>
    /// Milliseconds per "ideal" logic frame, computed based on <see cref="LogicFramesPerSecond"/>.
    /// </summary>
    public float MsPerLogicFrame { get; }

    internal readonly AssetLoadContext AssetLoadContext;
    public readonly AudioSystem AudioSystem;
    internal readonly ParticleSystemManager ParticleSystems;
    internal readonly ObjectCreationListManager ObjectCreationLists;
    public readonly Terrain.Terrain Terrain;
    public readonly Navigation.Navigation Navigation;
    public readonly Radar Radar;

    public readonly IGame Game;

    public SageGame SageGame => Game.SageGame;

    internal GameLogic GameLogic => Game.GameLogic;

    internal GameClient GameClient => Game.GameClient;

    public AssetStore AssetStore => AssetLoadContext.AssetStore;

    public readonly AI AI = new();

    public readonly IQuadtree<GameObject> Quadtree;

    // TODO: This is temporary until Scene3D and GameEngine are merged.
    public readonly IScene3D Scene3D;

    internal GameEngine(
        AssetLoadContext assetLoadContext,
        AudioSystem audioSystem,
        ParticleSystemManager particleSystems,
        ObjectCreationListManager objectCreationLists,
        Terrain.Terrain terrain,
        Navigation.Navigation navigation,
        Radar radar,
        IQuadtree<GameObject> quadtree,
        IScene3D scene,
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
        LogicFramesPerSecond = Game.SageGame.LogicFramesPerSecond();
        MsPerLogicFrame = Game.SageGame.MsPerLogicFrame();
    }
}
