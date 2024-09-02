using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Audio;
using OpenSage.Client;
using OpenSage.Content;
using OpenSage.Data.Sav;
using OpenSage.Logic;
using OpenSage.Network;
using OpenSage.Scripting;
using OpenSage.Terrain;

namespace OpenSage.Tests;

public abstract class StatePersisterTest : MockedGameTest
{
    protected const byte V1 = 0x01;
    protected const byte V2 = 0x02;
    protected const byte V3 = 0x03;

    protected virtual MemoryStream SaveData(byte[] data, byte version = V1)
    {
        return new MemoryStream([version, ..data]);
    }

    protected virtual MemoryStream SaveDataNoVersion(byte[] data)
    {
        return new MemoryStream(data);
    }
}

public abstract class MockedGameTest : IDisposable
{
    private protected TestGame Generals { get; } = new(SageGame.CncGenerals);
    private protected TestGame ZeroHour { get; } = new(SageGame.CncGeneralsZeroHour);

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private protected class TestGame : IGame
    {
        public SageGame SageGame { get; }
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
        public AudioSystem Audio { get; }
        public SelectionSystem Selection { get; }
        public GameContext Context { get; }

        public TestGame(SageGame game)
        {
            SageGame = game;
            AssetStore = new AssetStore(game, null, null, null, null, null, null, OnDemandAssetLoadStrategy.None);
            Context = new GameContext(AssetStore.LoadContext, null, null, null, null, null, null, null, Scene3D, this);

            AssetStore.PushScope();

            AssetStore.Ranks.Add(new RankTemplate { InternalId = 1 });

            PlayerManager = new PlayerManager(this);
            PlayerManager.OnNewGame([OpenSage.Data.Map.Player.CreateNeutralPlayer(), OpenSage.Data.Map.Player.CreateCivilianPlayer()], GameType.Skirmish);

            TerrainLogic = new TerrainLogic();
            TerrainLogic.SetHeightMapData(OpenSage.Data.Map.HeightMapData.Create(0, new ushort[2, 2] { { 0, 0 }, { 0, 0 } }));
        }

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
