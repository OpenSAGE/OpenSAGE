using System;
using System.Collections.Generic;
using OpenSage.Client;
using OpenSage.Content;
using OpenSage.Data.Sav;
using OpenSage.Logic;
using OpenSage.Network;
using OpenSage.Scripting;
using OpenSage.Terrain;

namespace OpenSage.Tests;

public abstract class MockedGameTest : IDisposable
{
    private protected TestGame Generals { get; } = new(SageGame.CncGenerals);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private protected class TestGame(SageGame game) : IGame
    {
        public SageGame SageGame { get; } = game;
        public AssetStore AssetStore { get; }
        public ContentManager ContentManager { get; }
        public SkirmishManager SkirmishManager { get; set; }
        public LobbyManager LobbyManager { get; }
        public ScriptingSystem Scripting { get; }
        public GameState GameState { get; }
        public GameStateMap GameStateMap { get; }
        public CampaignManager CampaignManager { get; }
        public TerrainLogic TerrainLogic { get; }
        public TerrainVisual TerrainVisual { get; }
        public GhostObjectManager GhostObjectManager { get; }
        public Scene2D Scene2D { get; }
        public Scene3D Scene3D { get; }
        public GameLogic GameLogic { get; }
        public GameClient GameClient { get; }
        public PlayerManager PlayerManager { get; }
        public TeamFactory TeamFactory { get; }
        public PartitionCellManager PartitionCellManager { get; }
        public bool InGame { get; }

        public void StartCampaign(string campaignName, string missionName)
        {
            throw new NotImplementedException();
        }

        public void StartSkirmishOrMultiPlayerGame(string mapFileName, IConnection connection, PlayerSetting[] playerSettings,
            int seed, bool isMultiPlayer)
        {
            throw new NotImplementedException();
        }

        public void StartSinglePlayerGame(string mapFileName)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PlayerTemplate> GetPlayableSides()
        {
            throw new NotImplementedException();
        }
    }
}
