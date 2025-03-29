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

public sealed class GameEngine : IGameEngine
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

    AssetLoadContext IGameEngine.AssetLoadContext => AssetLoadContext;
    internal AssetLoadContext AssetLoadContext { get; }
    public AudioSystem AudioSystem { get; }
    ParticleSystemManager IGameEngine.ParticleSystems => ParticleSystems;
    internal ParticleSystemManager ParticleSystems { get; }
    ObjectCreationListManager IGameEngine.ObjectCreationLists => ObjectCreationLists;
    internal ObjectCreationListManager ObjectCreationLists { get; }
    public Terrain.Terrain Terrain { get; }
    public Navigation.Navigation Navigation { get; }
    public Radar Radar { get; }

    public IGame Game { get; }

    public SageGame SageGame => Game.SageGame;

    GameLogic IGameEngine.GameLogic => GameLogic;
    internal GameLogic GameLogic => Game.GameLogic;

    GameClient IGameEngine.GameClient => GameClient;
    internal GameClient GameClient => Game.GameClient;

    public AssetStore AssetStore => AssetLoadContext.AssetStore;

    public AI AI { get; } = new();

    public IQuadtree<GameObject> Quadtree { get; }

    // TODO: This is temporary until Scene3D and GameEngine are merged.
    public IScene3D Scene3D { get; }

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
