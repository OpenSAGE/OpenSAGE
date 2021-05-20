using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using OpenSage.Audio;
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
using OpenSage.Graphics.Shaders;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Input;
using OpenSage.Input.Cursors;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using OpenSage.Network;
using OpenSage.Scripting;
using OpenSage.Utilities;
using Veldrid;
using Veldrid.ImageSharp;
using Player = OpenSage.Logic.Player;

namespace OpenSage
{
    public sealed class Game : DisposableBase
    {
        static Game()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        // TODO: These should be configurable at runtime with GameSpeed.

        // TODO: Revert this change. We haven't yet implemented interpolation between logic ticks,
        // so as a temporary workaround, we simply tick the logic at 30fps.
        //internal const double LogicUpdateInterval = 1000.0 / 5.0;
        internal const double LogicUpdateInterval = 1000.0 / 30.0;

        private readonly double _scriptingUpdateInterval;

        private readonly FileSystem _fileSystem;
        private readonly FileSystem _userDataFileSystem;
        private readonly FileSystem _userAppDataFileSystem;
        private readonly WndCallbackResolver _wndCallbackResolver;

        private readonly DeveloperModeView _developerModeView;

        private readonly TextureCopier _textureCopier;

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

        /// <summary>
        /// The current logic frame. Increments depending on game speed; by default once per 200ms.
        /// </summary>
        public ulong CurrentFrame { get; private set; }

        /// <summary>
        /// Is the game running?
        /// This is only false when the game is shutting down.
        /// </summary>
        public bool IsRunning { get; }

        public Action Restart { get; set; }

        /// <summary>
        /// Are we currently in a skirmish game?
        /// </summary>
        public bool InGame { get; private set; } = false;

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

            var mapFilename = replayFile.Header.Metadata.MapFile.Replace("userdata", _userDataFileSystem?.RootDirectory);
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
                string GetUserDataLeafNameFromRegistry(string defaultValue)
                {
                    foreach (var key in Definition.RegistryKeys)
                    {
                        var value = RegistryUtility.GetRegistryValue(new RegistryKeyPath(key.Key, "UserDataLeafName"));
                        if (value is not null)
                        {
                            return value;
                        }
                    }
                    return defaultValue;
                }

                // TODO: Move this to IGameDefinition?
                switch (SageGame)
                {
                    case SageGame.CncGenerals:
                        return "Command and Conquer Generals Data";

                    case SageGame.CncGeneralsZeroHour:
                        return "Command and Conquer Generals Zero Hour Data";

                    case SageGame.Ra3:
                        return GetUserDataLeafNameFromRegistry("Red Alert 3");

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

        public GameWindow Window { get; }

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

        public bool DeveloperModeEnabled { get; set; }

        public Texture LauncherImage { get; }

        public Game(
            GameInstallation installation,
            GraphicsBackend? preferredBackend) :
            this(installation, preferredBackend, new Configuration())
        {
        }

        public Game(
            GameInstallation installation,
            GraphicsBackend? preferredBackend,
            Configuration config)
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
                Window = AddDisposable(new GameWindow($"OpenSAGE - {installation.Game.DisplayName} - master",
                                                        100, 100, 1024, 768, preferredBackend, Configuration.UseFullscreen));
                GraphicsDevice = Window.GraphicsDevice;

                Panel = AddDisposable(new GamePanel(GraphicsDevice));

                InputMessageBuffer = new InputMessageBuffer();

                Definition = installation.Game;

                _fileSystem = AddDisposable(installation.CreateFileSystem());

                _mapTimer = AddDisposable(new DeltaTimer());
                _mapTimer.Start();

                _renderTimer = AddDisposable(new DeltaTimer());
                _renderTimer.Start();

                _wndCallbackResolver = new WndCallbackResolver();

                var standardGraphicsResources = AddDisposable(new StandardGraphicsResources(GraphicsDevice));
                var shaderResources = AddDisposable(new ShaderResourceManager(GraphicsDevice, standardGraphicsResources));
                GraphicsLoadContext = new GraphicsLoadContext(GraphicsDevice, standardGraphicsResources, shaderResources);

                AssetStore = new AssetStore(
                    SageGame,
                    _fileSystem,
                    LanguageUtility.ReadCurrentLanguage(Definition, _fileSystem.RootDirectory),
                    GraphicsDevice,
                    GraphicsLoadContext.StandardGraphicsResources,
                    GraphicsLoadContext.ShaderResources,
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
                    _userDataFileSystem = AddDisposable(new FileSystem(UserDataFolder));
                    ContentManager.UserDataFileSystem = _userDataFileSystem;
                }
                if (SageGame >= SageGame.Cnc3 && UserAppDataFolder is not null && Directory.Exists(UserAppDataFolder))
                {
                    var pathMapping = new Dictionary<string, string> { ["Maps"] = "data/maps/internal" };
                    _userAppDataFileSystem = AddDisposable(new FileSystem(UserAppDataFolder, null, pathMapping));
                    ContentManager.UserAppDataFileSystem = _userAppDataFileSystem;
                }
                if (ContentManager.UserMapsFileSystem is not null)
                {
                    new UserMapCache(ContentManager).Initialize(AssetStore);
                }

                _textureCopier = AddDisposable(new TextureCopier(this, GraphicsDevice.SwapchainFramebuffer.OutputDescription));

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
                OnPanelSizeChanged(this, EventArgs.Empty);

                GameSystems.ForEach(gs => gs.Initialize());

                Cursors = AddDisposable(new CursorManager(Window, AssetStore, ContentManager));
                Cursors.SetCursor("Arrow", _renderTimer.CurrentGameTime);

                _developerModeView = AddDisposable(new DeveloperModeView(this));

                LauncherImage = LoadLauncherImage();

                _mapTimer.Reset();

                LobbyManager = new LobbyManager(this);

                IsRunning = true;
                IsLogicRunning = true;
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
                Scene3D = LoadMap(shellMapName, null, GameType.SinglePlayer);
                Scripting.Active = true;
            }

            // TODO: MainMenu should never be null.
            if (Definition.MainMenu != null)
            {
                Definition.MainMenu.AddToScene(this, Scene2D, useShellMap);
            }
        }

        private Scene3D LoadMap(
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

            return new Scene3D(
                this,
                mapFile,
                entry.FilePath,
                Environment.TickCount,
                mapPlayers,
                mapTeams,
                mapScriptLists);
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

            Scene3D = LoadMap(mapFileName, playerSettings, gameType);

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
            CurrentFrame = 0;
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
            var firstMission = campaign.Missions.Single(x => x.Name == campaign.FirstMission);

            StartSinglePlayerGame(firstMission.Map);
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

            ShowMainMenu();
        }

        public void Run()
        {
            var totalGameTime = MapTime.TotalTime;
            _nextLogicUpdate = totalGameTime;
            _nextScriptingUpdate = totalGameTime;

            while (IsRunning)
            {
                if (!Window.PumpEvents())
                {
                    break;
                }

                if (DeveloperModeEnabled)
                {
                    _developerModeView.Tick();
                }
                else
                {
                    Update(Window.MessageQueue);

                    Panel.EnsureFrame(Window.ClientBounds);

                    Render();

                    _textureCopier.Execute(
                        Panel.Framebuffer.ColorTargets[0].Target,
                        GraphicsDevice.SwapchainFramebuffer);
                }

                Window.MessageQueue.Clear();

                GraphicsDevice.SwapBuffers();
            }

            // TODO: Cleanup resources.
        }

        public void Update(IEnumerable<InputMessage> messages)
        {
            // Update timers, input and UI state
            LocalLogicTick(messages);

            // Check global hotkeys
            CheckGlobalHotkeys();

            var totalGameTime = MapTime.TotalTime;

            // If the game is not paused and it's time to do a logic update, do so.
            if (IsLogicRunning && totalGameTime >= _nextLogicUpdate)
            {
                LogicTick(CurrentFrame);
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
            }

            // How close are we to the next logic frame?
            var tickT = (float) (1.0 - TimeSpanUtility.Max(_nextLogicUpdate - MapTime.TotalTime, TimeSpan.Zero)
                                     .TotalMilliseconds / LogicUpdateInterval);

            // We pass RenderTime to Scene2D so that the UI remains responsive even when the game is paused.
            Scene2D.LocalLogicTick(RenderTime, Scene3D?.LocalPlayer);
            Scene3D?.LocalLogicTick(MapTime, tickT);

            Audio.Update(Scene3D?.Camera);
            Cursors.Update(RenderTime);
        }

        private void CheckGlobalHotkeys()
        {
            if (Window.CurrentInputSnapshot.KeyEvents.Any(x => x.Down && x.Key == Veldrid.Key.F9))
            {
                ToggleLogicRunning();
            }

            if (!IsLogicRunning && Window.CurrentInputSnapshot.KeyEvents.Any(x => x.Down && x.Key == Veldrid.Key.F10))
            {
                Step();
            }

            if (Window.CurrentInputSnapshot.KeyEvents.Any(x => x.Down && x.Key == Veldrid.Key.F11))
            {
                DeveloperModeEnabled = !DeveloperModeEnabled;
            }

            if (Window.CurrentInputSnapshot.KeyEvents.Any(x => x.Down && x.Key == Veldrid.Key.Pause))
            {
                Restart?.Invoke();
            }

            if (Window.CurrentInputSnapshot.KeyEvents.Any(x => x.Down && x.Key == Veldrid.Key.Comma))
            {
                var rtsCam = Scene3D.CameraController as RtsCameraController;
                rtsCam.CanPlayerInputChangePitch = !rtsCam.CanPlayerInputChangePitch;
            }

            if (Window.CurrentInputSnapshot.KeyEvents.Any(x => x.Down && x.Key == Veldrid.Key.Enter && (x.Modifiers.HasFlag(ModifierKeys.Alt))))
            {
                Window.Fullscreen = !Window.Fullscreen;
            }
        }

        internal void LogicTick(ulong frame)
        {
            NetworkMessageBuffer?.Tick();

            foreach (var gameSystem in GameSystems)
            {
                gameSystem.LogicTick(CurrentFrame);
            }

            // TODO: What is the order?
            // TODO: Calculate time correctly.
            var timeInterval = new TimeInterval(MapTime.TotalTime, TimeSpan.FromMilliseconds(LogicUpdateInterval));
            Scene3D?.LogicTick(frame, timeInterval);

            CurrentFrame += 1;
        }

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

        internal void Render()
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
    }

    internal enum GameType
    {
        SinglePlayer = 0,
        MultiPlayer = 1,
        Skirmish = 2,
    }
}
