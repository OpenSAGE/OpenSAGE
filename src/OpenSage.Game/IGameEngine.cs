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

public interface IGameEngine
{
    /// <summary>
    /// "Ideal" logic frames per second, used for converting module data and animations.
    /// </summary>
    float LogicFramesPerSecond { get; }

    /// <summary>
    /// Milliseconds per "ideal" logic frame, computed based on <see cref="LogicFramesPerSecond"/>.
    /// </summary>
    float MsPerLogicFrame { get; }

    internal AssetLoadContext AssetLoadContext { get; }
    AudioSystem AudioSystem { get; }
    internal ParticleSystemManager ParticleSystems { get; }
    internal ObjectCreationListManager ObjectCreationLists { get; }
    Terrain.Terrain Terrain { get; }
    Navigation.Navigation Navigation { get; }
    Radar Radar { get; }
    IGame Game { get; }
    SageGame SageGame { get; }
    internal GameLogic GameLogic { get; }
    internal GameClient GameClient { get; }
    AssetStore AssetStore { get; }
    AI AI { get; }
    IQuadtree<GameObject> Quadtree { get; }
    IScene3D Scene3D { get; }
}
