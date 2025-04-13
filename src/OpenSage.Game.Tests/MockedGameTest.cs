using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Audio;
using OpenSage.Client;
using OpenSage.Content;
using OpenSage.Data.Sav;
using OpenSage.Graphics;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Input;
using OpenSage.Input.Cursors;
using OpenSage.IO;
using OpenSage.Logic;
using OpenSage.Network;
using OpenSage.Scripting;
using OpenSage.Terrain;
using Veldrid;

namespace OpenSage.Tests;

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
        public IGameDefinition Definition { get; }
        public SageGame SageGame { get; }
        public Configuration Configuration { get; }
        public string UserDataLeafName { get; }
        public string UserDataFolder { get; }
        public string UserAppDataFolder { get; }
        public GamePanel Panel { get; }
        public Viewport Viewport { get; }
        public CursorManager Cursors { get; }
        public GraphicsLoadContext GraphicsLoadContext { get; }
        public AssetStore AssetStore { get; }
        public ContentManager ContentManager { get; }
        public GraphicsDevice GraphicsDevice { get; }
        public InputMessageBuffer InputMessageBuffer { get; }
        public SkirmishManager SkirmishManager { get; set; }
        public LobbyManager LobbyManager { get; }
        public List<GameSystem> GameSystems { get; }
        public GraphicsSystem Graphics { get; }
        public ScriptingSystem Scripting { get; }
        public LuaScriptEngine Lua { get; set; }
        public GameState GameState { get; }
        public GameStateMap GameStateMap { get; }
        public CampaignManager CampaignManager { get; }
        public TerrainLogic TerrainLogic { get; }
        public TerrainVisual TerrainVisual { get; }
        public GhostObjectManager GhostObjectManager { get; }
        public bool IsRunning { get; }
        public Action Restart { get; set; }
        public Scene2D Scene2D { get; }
        public IScene3D Scene3D { get; set; }
        public NetworkMessageBuffer NetworkMessageBuffer { get; set; }
        public Texture LauncherImage { get; }
        public GameLogic GameLogic { get; }
        public GameClient GameClient { get; }
        public PlayerManager PlayerManager { get; }
        public TeamFactory TeamFactory { get; }
        public PartitionCellManager PartitionCellManager { get; }
        public bool InGame { get; }
        public float LogicUpdateScaleFactor { get; set; }
        public bool IsLogicRunning { get; set; }
        public TimeInterval MapTime { get; }
        public TimeInterval CurrentGameTime { get; }
        public TimeInterval RenderTime { get; }
        public TimeSpan CumulativeLogicUpdateError { get; }
        public OrderGeneratorSystem OrderGenerator { get; }
        public AudioSystem Audio { get; }
        public SelectionSystem Selection { get; }
        public IGameEngine GameEngine { get; }
#pragma warning disable CS0067 // The event 'Game.X' is never used
        public event EventHandler<GameUpdatingEventArgs> Updating;
        public event EventHandler RenderCompleted;
#pragma warning restore CS0067 // The event 'Game.X' is never used
        public void LoadSaveFile(FileSystemEntry entry)
        {
            throw new NotImplementedException();
        }

        public void LoadReplayFile(FileSystemEntry replayFileEntry)
        {
            throw new NotImplementedException();
        }

        public void ShowMainMenu()
        {
            throw new NotImplementedException();
        }

        public Window LoadWindow(string wndFileName)
        {
            throw new NotImplementedException();
        }

        public AptWindow LoadAptWindow(string aptFileName)
        {
            throw new NotImplementedException();
        }

        public void StartCampaign(string side)
        {
            throw new NotImplementedException();
        }

        public TestGame(SageGame game)
        {
            SageGame = game;
            AssetStore = new AssetStore(game, null, null, null, null, null, null, OnDemandAssetLoadStrategy.None);
            GameEngine = new GameEngine(AssetStore.LoadContext, null, null, null, null, null, null, null, Scene3D, this);

            AssetStore.PushScope();

            AssetStore.Ranks.Add(new RankTemplate { InternalId = 1 });

            PlayerManager = new PlayerManager(this);
            PlayerManager.OnNewGame([OpenSage.Data.Map.Player.CreateNeutralPlayer(), OpenSage.Data.Map.Player.CreateCivilianPlayer()], GameType.Skirmish);

            TerrainLogic = new TerrainLogic();
            TerrainLogic.SetHeightMapData(OpenSage.Data.Map.HeightMapData.Create(0, new ushort[2, 2] { { 0, 0 }, { 0, 0 } }));

            GameLogic = new GameLogic(this);
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

        public void EndGame()
        {
            throw new NotImplementedException();
        }

        public void StartRun()
        {
            throw new NotImplementedException();
        }

        public void Update(IEnumerable<InputMessage> messages)
        {
            throw new NotImplementedException();
        }

        public void Step()
        {
            throw new NotImplementedException();
        }

        public void Render()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Vector2 GetTopLeftUV()
        {
            throw new NotImplementedException();
        }

        public Vector2 GetBottomRightUV()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PlayerTemplate> GetPlayableSides()
        {
            throw new NotImplementedException();
        }

        public MappedImage GetMappedImage(string name)
        {
            throw new NotImplementedException();
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
