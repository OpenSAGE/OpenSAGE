using System.Collections.Generic;
using OpenSage.Audio;
using OpenSage.Client;
using OpenSage.Content;
using OpenSage.Data.Sav;
using OpenSage.Logic;
using OpenSage.Mathematics;
using OpenSage.Network;
using OpenSage.Scripting;

namespace OpenSage;

public interface IGame
{
    SageGame SageGame { get; }
    AssetStore AssetStore { get; }
    public ContentManager ContentManager { get; }
    public SkirmishManager SkirmishManager { get; set; }
    LobbyManager LobbyManager { get; }
    ScriptingSystem Scripting { get; }
    GameState GameState { get; }
    internal GameStateMap GameStateMap { get; }
    CampaignManager CampaignManager { get; }
    Terrain.TerrainLogic TerrainLogic { get; }
    Terrain.TerrainVisual TerrainVisual { get; }
    GhostObjectManager GhostObjectManager { get; }
    Scene2D Scene2D { get; }
    Scene3D Scene3D { get; }
    internal GameLogic GameLogic { get; }
    internal GameClient GameClient { get; }
    PlayerManager PlayerManager { get; }
    TeamFactory TeamFactory { get; }
    PartitionCellManager PartitionCellManager { get; }
    AudioSystem Audio { get; }
    public SelectionSystem Selection { get; }
    bool InGame { get; }
    GameEngine GameEngine { get; }

    void StartCampaign(string campaignName, string missionName);
    void StartSkirmishOrMultiPlayerGame(string mapFileName, IConnection connection, PlayerSetting[] playerSettings, int seed, bool isMultiPlayer);
    void StartSinglePlayerGame(string mapFileName);
    IEnumerable<PlayerTemplate> GetPlayableSides();

    // TODO: Move this somewhere where it could be configured for specific games;
    // e.g. for a replay we might want to use SageRandom.
    IRandom CreateRandom() => new SystemRandom();
}
