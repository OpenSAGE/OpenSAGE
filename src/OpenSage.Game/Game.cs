﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using OpenSage.Audio;
using OpenSage.Client;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Data.Apt;
using OpenSage.Data.Map;
using OpenSage.Data.Rep;
using OpenSage.Data.Sav;
using OpenSage.Data.Wnd;
using OpenSage.Diagnostics;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Input;
using OpenSage.Input.Cursors;
using OpenSage.IO;
using OpenSage.Logic;
using OpenSage.Mathematics;
using OpenSage.Network;
using OpenSage.Rendering;
using OpenSage.Scripting;
using OpenSage.Utilities;
using Veldrid;
using Veldrid.ImageSharp;

namespace OpenSage
{
    public sealed class Game : DisposableBase, IGame
    {
        static Game()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        // TODO: These should be configurable at runtime with GameSpeed.

        public const float LogicFramesPerSecond = 30.0f;

        // TODO: Revert this change. We haven't yet implemented interpolation between logic ticks,
        // so as a temporary workaround, we simply tick the logic at 30fps.
        //internal const double LogicUpdateInterval = 1000.0 / 5.0;
        internal const float LogicUpdateInterval = 1000.0f / LogicFramesPerSecond;

        private readonly double _scriptingUpdateInterval;

        private readonly FileSystem _fileSystem;
        private readonly WndCallbackResolver _wndCallbackResolver;

        internal readonly CursorManager Cursors;

        internal GraphicsLoadContext GraphicsLoadContext { get; }
        public AssetStore AssetStore { get; }

        public ContentManager ContentManager { get; }

        public GraphicsDevice GraphicsDevice { get; }

        public InputMessageBuffer InputMessageBuffer { get; }

        public SkirmishManager SkirmishManager { get; set; }

        public LobbyManager LobbyManager { get; }

        internal List<GameSystem> GameSystems { get; }

        /// <summary>
        /// Gets the graphics system.
        /// </summary>
        public GraphicsSystem Graphics { get; }

        /// <summary>
        /// Gets the scripting system.
        /// </summary>
        public ScriptingSystem Scripting { get; }

        /// <summary>
        /// Load lua script engine.
        /// </summary>
        public LuaScriptEngine Lua { get; set; }

        /// <summary>
        /// Gets the selection system.
        /// </summary>
        public SelectionSystem Selection { get; }

        /// <summary>
        /// Gets the order generator system.
        /// </summary>
        public OrderGeneratorSystem OrderGenerator { get; }

        /// <summary>
        /// Gets the audio system
        /// </summary>
        public AudioSystem Audio { get; }

        public GameState GameState { get; } = new GameState();

        GameStateMap IGame.GameStateMap => GameStateMap;

        internal GameStateMap GameStateMap { get; } = new GameStateMap();

        public CampaignManager CampaignManager { get; } = new CampaignManager();

        public Terrain.TerrainLogic TerrainLogic { get; } = new Terrain.TerrainLogic();

        public Terrain.TerrainVisual TerrainVisual { get; } = new Terrain.TerrainVisual();

        public GhostObjectManager GhostObjectManager { get; } = new GhostObjectManager();

        /// <summary>
        /// Is the game running?
        /// This is only false when the game is shutting down.
        /// </summary>
        public bool IsRunning { get; private set; }

        public Action Restart { get; set; }

        /// <summary>
        /// Are we currently in a skirmish game?
        /// </summary>
        public bool InGame { get; private set; } = false;

        public event EventHandler<GameUpdatingEventArgs> Updating;

        /// <summary>
        /// Fired when a <see cref="Render"/> completes, but before
        /// <see cref="Panel"/>'s <see cref="GamePanel.Framebuffer"/>
        /// is copied to <see cref="GraphicsDevice.SwapchainFramebuffer"/>.
        /// Useful for drawing additional overlays.
        /// </summary>
        public event EventHandler RenderCompleted;

        public void LoadSaveFile(FileSystemEntry entry)
        {
            SaveFile.Load(entry, this);
        }

        public void LoadReplayFile(FileSystemEntry replayFileEntry)
        {
            var replayFile = ReplayFile.FromFileSystemEntry(replayFileEntry);

            var mapFilename = replayFile.Header.Metadata.MapFile.Replace("userdata", ContentManager.UserDataFileSystem?.RootDirectory);
            mapFilename = FileSystem.NormalizeFilePath(mapFilename);
            var mapName = mapFilename.Substring(mapFilename.LastIndexOf(Path.DirectorySeparatorChar));

            var pSettings = ParseReplayMetaToPlayerSettings(replayFile.Header.Metadata.Slots);

            StartSkirmishOrMultiPlayerGame(
                mapFilename + mapName + ".map",
                new ReplayConnection(replayFile),
                pSettings.ToArray(),
                replayFile.Header.Metadata.SD,
                isMultiPlayer: false); // TODO
        }

        private List<PlayerSetting> ParseReplayMetaToPlayerSettings(ReplaySlot[] slots)
        {
            var random = new Random();

            // TODO: set the correct factions & colors
            var pSettings = new List<PlayerSetting>();

            var availableColors = new HashSet<MultiplayerColor>(AssetStore.MultiplayerColors);

            foreach (var slot in slots)
            {
                var colorIndex = (int) slot.Color;
                if (colorIndex >= 0 && colorIndex < AssetStore.MultiplayerColors.Count)
                {
                    availableColors.Remove(AssetStore.MultiplayerColors.GetByIndex(colorIndex));
                }
            }

            foreach (var slot in slots)
            {
                var owner = PlayerOwner.Player;

                if (slot.SlotType == ReplaySlotType.Empty)
                {
                    continue;
                }

                var factionIndex = slot.Faction;
                if (factionIndex == -1) // random
                {
                    var maxFactionIndex = AssetStore.PlayerTemplates.Count;
                    var minFactionIndex = 2; // 0 and 1 are civilian and observer

                    var diff = maxFactionIndex - minFactionIndex;
                    factionIndex = minFactionIndex + (random.Next() % diff);
                }

                var faction = AssetStore.PlayerTemplates.GetByIndex(factionIndex).Side;

                ColorRgb color;

                var colorIndex = (int) slot.Color;
                if (colorIndex >= 0 && colorIndex < AssetStore.MultiplayerColors.Count)
                {
                    color = AssetStore.MultiplayerColors.GetByIndex(slot.Color).RgbColor;
                }
                else
                {
                    var multiplayerColor = availableColors.First();
                    color = multiplayerColor.RgbColor;
                    availableColors.Remove(multiplayerColor);
                }

                if (slot.SlotType == ReplaySlotType.Computer)
                {
                    switch (slot.ComputerDifficulty)
                    {
                        case ReplaySlotDifficulty.Easy:
                            owner = PlayerOwner.EasyAi;
                            break;

                        case ReplaySlotDifficulty.Medium:
                            owner = PlayerOwner.MediumAi;
                            break;

                        case ReplaySlotDifficulty.Hard:
                            owner = PlayerOwner.HardAi;
                            break;
                    }

                }
                pSettings.Add(new PlayerSetting(slot.StartPosition, faction, color, slot.Team, owner, slot.HumanName));
            }

            return pSettings;
        }

        /// <summary>
        /// Is the game running logic updates?
        /// Automatically starts and stops the map timer.
        /// </summary>
        public bool IsLogicRunning
        {
            get => _isLogicRunning;
            internal set
            {
                _isLogicRunning = value;
                _isStepping = false;

                if (!_isLogicRunning)
                {
                    _mapTimer.Pause();
                }
                else
                {
                    _mapTimer.Continue();
                }
            }
        }
        private bool _isLogicRunning;

        // Are we in the middle of stepping until the next logic frame?
        private bool _isStepping;

        // Measures time when IsLogicRunning == true.
        // Is reset when the map changes.
        private readonly DeltaTimer _mapTimer;

        /// <summary>
        /// Used for RenderDoc integration
        /// </summary>
        public static RenderDoc RenderDoc;

        /// <summary>
        /// The amount of time the game has been in this map while running logic updates.
        /// </summary>
        public TimeInterval MapTime { get; private set; }

        // Increments continuously.
        // Never stops, never resets.
        // Used for FPS calculations and rendering-related things that should advanced even when the game is paused.
        private readonly DeltaTimer _renderTimer;

        public TimeInterval CurrentGameTime => _renderTimer.CurrentGameTime;

        /// <summary>
        /// The amount of time the game has been rendering frames.
        /// </summary>
        public TimeInterval RenderTime { get; private set; }

        // The time of the next logic update.
        private TimeSpan _nextLogicUpdate;

        // When is the next scripting update?
        private TimeSpan _nextScriptingUpdate;

        // TODO: Move this to somewhere else, or remove it.
        public TimeSpan CumulativeLogicUpdateError { get; private set; }

        public IGameDefinition Definition { get; }
        public SageGame SageGame => Definition.Game;

        public Configuration Configuration { get; private set; }

        public string UserDataLeafName
        {
            get
            {
                // TODO: Move this to IGameDefinition?
                switch (SageGame)
                {
                    case SageGame.CncGenerals:
                        return "Command and Conquer Generals Data";

                    case SageGame.CncGeneralsZeroHour:
                        return "Command and Conquer Generals Zero Hour Data";

                    default:
                        return AssetStore.GameData.Current.UserDataLeafName;
                }
            }
        }

        public string UserDataFolder => UserDataLeafName != null
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), UserDataLeafName)
            : null;

        public string UserAppDataFolder => UserDataLeafName != null
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), UserDataLeafName)
            : null;

        public GamePanel Panel { get; }

        public Viewport Viewport { get; private set; }

        public Scene2D Scene2D { get; }

        private Scene3D _scene3D;
        public Scene3D Scene3D
        {
            get => _scene3D;
            set
            {
                foreach (var gameSystem in GameSystems)
                {
                    gameSystem.OnSceneChanging();
                }

                RemoveAndDispose(ref _scene3D);
                _scene3D = AddDisposable(value);

                foreach (var gameSystem in GameSystems)
                {
                    gameSystem.OnSceneChanged();
                }

                if (value != null)
                {
                    PartitionCellManager.OnNewGame();
                }
            }
        }

        private NetworkMessageBuffer _networkMessageBuffer;
        public NetworkMessageBuffer NetworkMessageBuffer
        {
            get => _networkMessageBuffer;
            // TODO: Make this private again later.
            set
            {
                _networkMessageBuffer?.Dispose();
                _networkMessageBuffer = value;
            }
        }

        public Texture LauncherImage { get; }

        // currently, the only way to implement internal interface properties is explicitly
        GameLogic IGame.GameLogic => GameLogic;

        internal readonly GameLogic GameLogic;

        GameClient IGame.GameClient => GameClient;

        internal readonly GameClient GameClient;

        public PlayerManager PlayerManager { get; }

        TeamFactory IGame.TeamFactory => TeamFactory;

        internal readonly TeamFactory TeamFactory;

        public PartitionCellManager PartitionCellManager { get; }

        public GameContext Context => Scene3D.GameContext;

        public Game(GameInstallation installation)
            : this(installation, null, new Configuration(), null)
        {
        }

        public Game(
            GameInstallation installation,
            GraphicsBackend? preferredBackend,
            Configuration config,
            GameWindow window)
        {
            using (GameTrace.TraceDurationEvent("Game()"))
            {
                Configuration = config;

                // TODO: This should probably be done in some dynamic way.
                // We can't change our backend at runtime though. See NeoDemo.cs
                if (Configuration.UseRenderDoc && RenderDoc == null)
                {
                    RenderDoc.Load(out RenderDoc);
                }

                // TODO: Read game version from assembly metadata or .git folder
                // TODO: Set window icon.
                GraphicsDevice = AddDisposable(GraphicsDeviceUtility.CreateGraphicsDevice(preferredBackend, window));

                Panel = AddDisposable(new GamePanel(GraphicsDevice));

                InputMessageBuffer = new InputMessageBuffer();

                InputMessageBuffer.Handlers.Add(
                    new CallbackMessageHandler(
                        HandlingPriority.Engine,
                        message =>
                        {
                            if (message.MessageType == InputMessageType.KeyDown && message.Value.Key == Key.F9)
                            {
                                ToggleLogicRunning();
                                return InputMessageResult.Handled;
                            }

                            if (!IsLogicRunning && message.MessageType == InputMessageType.KeyDown && message.Value.Key == Key.F10)
                            {
                                Step();
                                return InputMessageResult.Handled;
                            }

                            if (message.MessageType == InputMessageType.KeyDown && message.Value.Key == Key.Pause)
                            {
                                Restart?.Invoke();
                                return InputMessageResult.Handled;
                            }

                            if (message.MessageType == InputMessageType.KeyDown && message.Value.Key == Key.Comma)
                            {
                                var rtsCam = Scene3D.CameraController as RtsCameraController;
                                rtsCam.CanPlayerInputChangePitch = !rtsCam.CanPlayerInputChangePitch;
                                return InputMessageResult.Handled;
                            }

                            return InputMessageResult.NotHandled;
                        }));

                Definition = installation.Game;

                _fileSystem = AddDisposable(installation.CreateFileSystem());

                _mapTimer = AddDisposable(new DeltaTimer());
                _mapTimer.Start();

                _renderTimer = AddDisposable(new DeltaTimer());
                _renderTimer.Start();

                _wndCallbackResolver = new WndCallbackResolver();

                var standardGraphicsResources = AddDisposable(new StandardGraphicsResources(GraphicsDevice));
                var shaderSetStore = AddDisposable(new ShaderSetStore(GraphicsDevice, RenderPipeline.GameOutputDescription));
                var shaderResources = AddDisposable(new ShaderResourceManager(GraphicsDevice, standardGraphicsResources, shaderSetStore));
                GraphicsLoadContext = new GraphicsLoadContext(GraphicsDevice, standardGraphicsResources, shaderResources, shaderSetStore);

                AssetStore = new AssetStore(
                    SageGame,
                    _fileSystem,
                    LanguageUtility.ReadCurrentLanguage(Definition, _fileSystem),
                    GraphicsDevice,
                    GraphicsLoadContext.StandardGraphicsResources,
                    GraphicsLoadContext.ShaderResources,
                    shaderSetStore,
                    Definition.CreateAssetLoadStrategy());

                // TODO
                AssetStore.PushScope();

                ContentManager = AddDisposable(new ContentManager(
                    this,
                    _fileSystem,
                    GraphicsDevice,
                    SageGame));

                // Create file system for user data folder and load user maps.
                // This has to be done after the ContentManager is initialized and
                // the GameData.ini file has been parsed because we don't know
                // the UserDataFolder before then.
                if (UserDataFolder is not null && Directory.Exists(UserDataFolder))
                {
                    ContentManager.UserDataFileSystem = AddDisposable(new DiskFileSystem(UserDataFolder));
                }
                if (SageGame >= SageGame.Bfme && UserAppDataFolder is not null && Directory.Exists(UserAppDataFolder))
                {
                    ContentManager.UserDataFileSystem = AddDisposable(new DiskFileSystem(UserAppDataFolder));
                }
                // TODO
                //if (SageGame >= SageGame.Cnc3 && UserAppDataFolder is not null && Directory.Exists(UserAppDataFolder))
                //{
                //    var userDataFileSystem = ContentManager.UserDataFileSystem ?? AddDisposable(new DiskFileSystem(UserAppDataFolder));

                //    ContentManager.UserDataFileSystem = AddDisposable(new CompositeFileSystem(
                //        userDataFileSystem,
                //        new VirtualFileSystem(
                //            @"data\\maps\\internal",
                //            new DiskFileSystem(UserAppDataFolder))));
                //}
                if (ContentManager.UserDataFileSystem is not null)
                {
                    new UserMapCache(ContentManager).Initialize(AssetStore);
                }

                GameSystems = new List<GameSystem>();

                Audio = AddDisposable(new AudioSystem(this));

                Graphics = AddDisposable(new GraphicsSystem(this));

                _scriptingUpdateInterval = 1000.0 / installation.Game.ScriptingTicksPerSecond;

                Scripting = AddDisposable(new ScriptingSystem(this));

                Lua = AddDisposable(new LuaScriptEngine(this));

                Scene2D = new Scene2D(this);

                Selection = AddDisposable(new SelectionSystem(this));

                OrderGenerator = AddDisposable(new OrderGeneratorSystem(this));

                Panel.ClientSizeChanged += OnPanelSizeChanged;
                Panel.EnsureFrame(window?.ClientBounds ?? new Mathematics.Rectangle(0, 0, 1, 1)); // fallback set for sav2json tool

                GameSystems.ForEach(gs => gs.Initialize());

                Cursors = AddDisposable(new CursorManager(AssetStore, ContentManager, window));
                Cursors.SetCursor("Arrow", CurrentGameTime);

                LauncherImage = LoadLauncherImage();

                _mapTimer.Reset();

                LobbyManager = new LobbyManager(this);

                IsRunning = true;
                IsLogicRunning = true;

                GameLogic = AddDisposable(new GameLogic(this));

                GameClient = AddDisposable(new GameClient(this));

                PlayerManager = new PlayerManager(this);

                TeamFactory = new TeamFactory(this);

                PartitionCellManager = new PartitionCellManager(this);
            }
        }

        private void OnPanelSizeChanged(object sender, EventArgs e)
        {
            var newSize = Panel.ClientBounds.Size;

            Viewport = new Viewport(
                0,
                0,
                newSize.Width,
                newSize.Height,
                0,
                1);

            Scene2D.WndWindowManager.OnViewportSizeChanged(newSize);
            Scene2D.AptWindowManager.OnViewportSizeChanged(newSize);

            Scene3D?.Camera.OnViewportSizeChanged();
        }

        public void ShowMainMenu()
        {
            var useShellMap = Configuration.LoadShellMap;
            if (useShellMap)
            {
                var shellMapName = AssetStore.GameData.Current.ShellMapName;
                LoadMap(shellMapName, null, GameType.SinglePlayer);
                Scripting.Active = true;
            }

            // TODO: MainMenu should never be null.
            if (Definition.MainMenu != null)
            {
                Definition.MainMenu.AddToScene(this, Scene2D, useShellMap);
            }
        }

        private void LoadMap(
            string mapPath,
            PlayerSetting[] playerSettings,
            GameType gameType)
        {
            var entry = ContentManager.GetMapEntry(mapPath);
            var mapFile = MapFile.FromFileSystemEntry(entry);

            SidesListUtility.SetupGameSides(
                this,
                mapFile,
                playerSettings,
                gameType,
                out var mapPlayers,
                out var mapTeams,
                out var mapScriptLists);

            TerrainLogic.SetHeightMapData(mapFile.HeightMapData);

            new Scene3D(
                this,
                mapFile,
                entry.FilePath,
                Environment.TickCount,
                mapPlayers,
                mapTeams,
                mapScriptLists,
                gameType);
        }

        public Window LoadWindow(string wndFileName)
        {
            var entry = ContentManager.FileSystem.GetFile(Path.Combine("Window", wndFileName));
            if (entry == null)
            {
                throw new Exception($"Window file {wndFileName} was not found.");
            }
            var wndFile = WndFile.FromFileSystemEntry(entry, AssetStore);
            return new Window(wndFile, this, _wndCallbackResolver);
        }

        public AptWindow LoadAptWindow(string aptFileName)
        {
            var entry = ContentManager.FileSystem.GetFile(aptFileName);
            var aptFile = AptFile.FromFileSystemEntry(entry);
            return new AptWindow(this, ContentManager, aptFile);
        }

        private void StartGame(
            string mapFileName,
            IConnection connection,
            PlayerSetting[] playerSettings,
            GameType gameType,
            int seed)
        {
            Audio.StopCurrentMusicTrack();
            InGame = true;

            // TODO: Loading screen.
            while (Scene2D.WndWindowManager.OpenWindowCount > 0)
            {
                Scene2D.WndWindowManager.PopWindow();
            }

            while (Scene2D.AptWindowManager.OpenWindowCount > 0)
            {
                Scene2D.AptWindowManager.PopWindow();
            }

            LoadMap(mapFileName, playerSettings, gameType);

            if (Scene3D == null)
            {
                throw new Exception($"Failed to load Scene3D \"{mapFileName}\"");
            }

            if (gameType != GameType.SinglePlayer)
            {
                for (var i = 0; i < playerSettings.Length; i++)
                {
                    Scene3D.CreateSkirmishPlayerStartingBuilding(
                        playerSettings[i],
                        Scene3D.Players[i + 2]);
                }
            }

            if (Scene3D.LocalPlayer.SelectedUnits.Count > 0)
            {
                var mainUnit = Scene3D.LocalPlayer.SelectedUnits.First();
                Scene3D.CameraController.GoToObject(mainUnit);
            }
            else
            {
                //Scene3D.CameraController.TerrainPosition = playerStartPosition;
            }

            Scene3D.FrustumCulling = true;

            NetworkMessageBuffer = new NetworkMessageBuffer(this, connection);

            if (Definition.ControlBar != null)
            {
                Scene2D.ControlBar = Definition.ControlBar.Create(Scene3D.LocalPlayer.Side, this);
                Scene2D.ControlBar.AddToScene(Scene2D);
            }

            if (Definition.CommandListOverlay != null)
            {
                Scene2D.UnitOverlay = Definition.CommandListOverlay.Create(this);
            }

            // Reset everything, and run the first update on the first frame.
            _mapTimer.Reset();
            _nextLogicUpdate = TimeSpan.Zero;
            _nextScriptingUpdate = TimeSpan.Zero;
            CumulativeLogicUpdateError = TimeSpan.Zero;

            // Scripts should be enabled in all games, even replays
            Scripting.Active = true;
        }

        public void StartCampaign(string side)
        {
            // TODO: Difficulty

            var campaign = AssetStore.CampaignTemplates.GetByName(side);
            var firstMission = campaign.Missions[campaign.FirstMission];

            StartSinglePlayerGame(firstMission.Map);
        }

        public void StartCampaign(string campaignName, string missionName)
        {
            var campaign = AssetStore.CampaignTemplates.GetByName(campaignName);
            var mission = campaign.Missions[missionName];

            StartSinglePlayerGame(mission.Map);
        }

        public void StartSkirmishOrMultiPlayerGame(
            string mapFileName,
            IConnection connection,
            PlayerSetting[] playerSettings,
            int seed,
            bool isMultiPlayer)
        {
            StartGame(
                mapFileName,
                connection,
                playerSettings,
                isMultiPlayer ? GameType.MultiPlayer : GameType.Skirmish,
                seed);
        }

        public void StartSinglePlayerGame(string mapFileName)
        {
            StartGame(
                mapFileName,
                new EchoConnection(),
                null,
                GameType.SinglePlayer,
                seed: Environment.TickCount);
        }

        public void EndGame()
        {
            // TODO: there's a huge memory leak somewhere here...
            // Hopefully it will be fixed when we refactor ContentManager.

            Audio.StopCurrentMusicTrack();

            Scene3D.Dispose();
            Scene3D = null;

            NetworkMessageBuffer.Dispose();
            NetworkMessageBuffer = null;

            while (Scene2D.WndWindowManager.OpenWindowCount > 0)
            {
                Scene2D.WndWindowManager.PopWindow();
            }
            while (Scene2D.AptWindowManager.OpenWindowCount > 0)
            {
                Scene2D.AptWindowManager.PopWindow();
            }

            InGame = false;

            ShowMainMenu();
        }

        public void StartRun()
        {
            var totalGameTime = MapTime.TotalTime;
            _nextLogicUpdate = totalGameTime;
            _nextScriptingUpdate = totalGameTime;
        }

        public void Update(IEnumerable<InputMessage> messages)
        {
            // Update timers, input and UI state
            LocalLogicTick(messages);

            var totalGameTime = MapTime.TotalTime;

            // If the game is not paused and it's time to do a logic update, do so.
            if (IsLogicRunning && totalGameTime >= _nextLogicUpdate)
            {
                LogicTick();
                CumulativeLogicUpdateError += (totalGameTime - _nextLogicUpdate);
                // Logic updates happen at 5Hz.
                _nextLogicUpdate += TimeSpan.FromMilliseconds(LogicUpdateInterval);

                if (_isStepping)
                {
                    IsLogicRunning = false;
                }
            }

            // TODO: Which update should be performed first?
            if (IsLogicRunning && totalGameTime >= _nextScriptingUpdate)
            {
                Scripting.ScriptingTick();
                // Scripting updates happen at 30Hz / 5Hz depending on game.
                _nextScriptingUpdate += TimeSpan.FromMilliseconds(_scriptingUpdateInterval);
            }
        }

        private void LocalLogicTick(IEnumerable<InputMessage> messages)
        {
            _mapTimer.Update();
            MapTime = _mapTimer.CurrentGameTime;

            _renderTimer.Update();
            RenderTime = _renderTimer.CurrentGameTime;

            InputMessageBuffer.PumpEvents(messages);

            // Prevent virtual Update() call when the game has already started, it's only needed in the menu
            if (SkirmishManager != null && SkirmishManager.Settings.Status != SkirmishGameStatus.Started)
            {
                SkirmishManager?.Update();

                // TODO: This is a hack just to get MapTime correct immediately after we start a game.
                _mapTimer.Update();
                MapTime = _mapTimer.CurrentGameTime;
            }

            // How close are we to the next logic frame?
            var tickT = (float) (1.0 - TimeSpanUtility.Max(_nextLogicUpdate - MapTime.TotalTime, TimeSpan.Zero)
                                     .TotalMilliseconds / LogicUpdateInterval);

            // We pass RenderTime to Scene2D so that the UI remains responsive even when the game is paused.
            Scene2D.LocalLogicTick(RenderTime, Scene3D?.LocalPlayer);
            Scene3D?.LocalLogicTick(MapTime, tickT);

            Audio.Update(Scene3D?.Camera);
            Cursors.Update(RenderTime);

            Updating?.Invoke(this, new GameUpdatingEventArgs(RenderTime));
        }

        internal void LogicTick()
        {
            GameLogic.Update();

            NetworkMessageBuffer?.Tick();

            // TODO: What is the order?
            // TODO: Calculate time correctly.
            var timeInterval = GetTimeInterval();
            Scene3D?.LogicTick(timeInterval);

            PartitionCellManager.Update();
        }

        internal TimeInterval GetTimeInterval() => new TimeInterval(MapTime.TotalTime, TimeSpan.FromMilliseconds(LogicUpdateInterval));

        public void ToggleLogicRunning()
        {
            IsLogicRunning = !IsLogicRunning;
            _isStepping = false;
        }

        public void Step()
        {
            IsLogicRunning = true;
            _isStepping = true;
        }

        public void Render()
        {
            Graphics.Draw(RenderTime);
            RenderCompleted?.Invoke(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            base.Dispose(disposeManagedResources);

            if (UPnP.Status == UPnPStatus.PortsForwarded)
            {
                UPnP.RemovePortForwardingAsync().Wait();
            }

            GC.Collect();
        }

        private Texture LoadLauncherImage()
        {
            var launcherImagePath = Definition.LauncherImagePath;
            if (launcherImagePath != null)
            {
                var prefixLang = Definition.LauncherImagePrefixLang;
                if (prefixLang)
                {
                    launcherImagePath = ContentManager.Language + launcherImagePath;
                }

                var launcherImageEntry = _fileSystem.GetFile(launcherImagePath);
                if (launcherImageEntry != null)
                {
                    return AddDisposable(new ImageSharpTexture(launcherImageEntry.Open()).CreateDeviceTexture(
                        GraphicsDevice, GraphicsDevice.ResourceFactory));
                }
            }

            return null;
        }

        // TODO: Move these to somewhere more suitable.
        internal Vector2 GetTopLeftUV()
        {
            return GraphicsDevice.IsUvOriginTopLeft ?
                new Vector2(0, 0) :
                new Vector2(0, 1);
        }

        internal Vector2 GetBottomRightUV()
        {
            return GraphicsDevice.IsUvOriginTopLeft ?
                new Vector2(1, 1) :
                new Vector2(1, 0);
        }

        // TODO: Move this to somewhere better.
        public IEnumerable<PlayerTemplate> GetPlayableSides() => AssetStore.PlayerTemplates.Where(x => x.PlayableSide);

        // TODO: Remove this.
        public MappedImage GetMappedImage(string name) => AssetStore.MappedImages.GetByName(name);

        public void Exit()
        {
            // TODO: Ensure we've cleaned up all resources.
            IsRunning = false;
        }
    }

    internal enum GameType
    {
        SinglePlayer = 0,
        MultiPlayer = 1,
        Skirmish = 2,
    }

    public sealed class GameUpdatingEventArgs : EventArgs
    {
        public TimeInterval GameTime { get; }

        public GameUpdatingEventArgs(TimeInterval gameTime)
        {
            GameTime = gameTime;
        }
    }
}
