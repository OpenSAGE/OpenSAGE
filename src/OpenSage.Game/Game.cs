using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Audio;
using OpenSage.Data;
using OpenSage.Diagnostics;
using OpenSage.Graphics;
using OpenSage.Gui.Wnd;
using OpenSage.Input;
using OpenSage.Logic;
using OpenSage.Mathematics;
using OpenSage.Network;
using OpenSage.Scripting;
using OpenSage.Utilities;
using Veldrid;
using Veldrid.ImageSharp;
using Player = OpenSage.Logic.Player;
using OpenSage.Data.Rep;

namespace OpenSage
{
    public sealed class Game : DisposableBase
    {
        // TODO: These should be configurable at runtime with GameSpeed.
        private const double LogicUpdateInterval = 1000.0 / 5.0;
        private const double ScriptingUpdateInterval = 1000.0 / 30.0;

        private readonly FileSystem _fileSystem;
        private readonly WndCallbackResolver _wndCallbackResolver;

        private readonly Dictionary<string, Cursor> _cachedCursors;
        private Cursor _currentCursor;

        private readonly DeveloperModeView _developerModeView;

        private readonly TextureCopier _textureCopier;

        public ContentManager ContentManager { get; }

        public GraphicsDevice GraphicsDevice { get; }

        public InputMessageBuffer InputMessageBuffer { get; }

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

        public void LoadReplayFile(FileSystemEntry replayFileEntry)
        {
            var replayFile = ReplayFile.FromFileSystemEntry(replayFileEntry);

            // TODO: This probably isn't right.
            var mapFilenameParts = replayFile.Header.Metadata.MapFile.Split('/');
            var mapFilename = $"Maps\\{mapFilenameParts[1]}\\{mapFilenameParts[1]}.map";

            var pSettings = ParseReplayMetaToPlayerSettings(replayFile.Header.Metadata.Slots);

            StartMultiPlayerGame(
                mapFilename,
                new ReplayConnection(replayFile),
                pSettings.ToArray(),
                0);
        }

        private List<PlayerSetting?> ParseReplayMetaToPlayerSettings(ReplaySlot[] slots)
        {
            var random = new Random();

            // TODO: set the correct factions & colors
            var pSettings = new List<PlayerSetting?>();

            var availableColors = new HashSet<Data.Ini.MultiplayerColor>(ContentManager.IniDataContext.MultiplayerColors);

            foreach (var slot in slots)
            {
                var colorIndex = (int) slot.Color;
                if (colorIndex >= 0 && colorIndex < ContentManager.IniDataContext.MultiplayerColors.Count)
                {
                    availableColors.Remove(ContentManager.IniDataContext.MultiplayerColors[colorIndex]);
                }
            }

            foreach (var slot in slots)
            {
                var owner = PlayerOwner.Player;

                if (slot.SlotType == ReplaySlotType.Empty)
                {
                    pSettings.Add(null);
                    continue;
                }

                var factionIndex = slot.Faction;
                if (factionIndex == -1) // random
                {
                    var maxFactionIndex = ContentManager.IniDataContext.PlayerTemplates.Count;
                    var minFactionIndex = 2; // 0 and 1 are civilian and observer

                    var diff = maxFactionIndex - minFactionIndex;
                    factionIndex = minFactionIndex + (random.Next() % diff);
                }

                var faction = ContentManager.IniDataContext.PlayerTemplates[factionIndex];

                var color = new ColorRgb(0, 0, 0);

                var colorIndex = (int) slot.Color;
                if (colorIndex >= 0 && colorIndex < ContentManager.IniDataContext.MultiplayerColors.Count)
                {
                    color = ContentManager.IniDataContext.MultiplayerColors[(int) slot.Color].RgbColor;
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
                pSettings.Add(new PlayerSetting(slot.StartPosition, faction.Side, color, owner, slot.HumanName));

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
                // TODO: Move this to IGameDefinition?
                switch (SageGame)
                {
                    case SageGame.CncGeneralsZeroHour:
                        return "Command and Conquer Generals Zero Hour Data";

                    default:
                        return ContentManager.IniDataContext.GameData.UserDataLeafName;
                }
            }
        }

        public string UserDataFolder => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            UserDataLeafName);

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

        public Player CivilianPlayer { get; private set; }

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

                _cachedCursors = new Dictionary<string, Cursor>();

                _wndCallbackResolver = new WndCallbackResolver();

                ContentManager = AddDisposable(new ContentManager(
                    this,
                    _fileSystem,
                    GraphicsDevice,
                    SageGame,
                    _wndCallbackResolver));

                _textureCopier = AddDisposable(new TextureCopier(this, GraphicsDevice.SwapchainFramebuffer.OutputDescription));

                GameSystems = new List<GameSystem>();

                Audio = AddDisposable(new AudioSystem(this));

                Graphics = AddDisposable(new GraphicsSystem(this));

                Scripting = AddDisposable(new ScriptingSystem(this));

                Lua = AddDisposable(new LuaScriptEngine(this));

                Scene2D = new Scene2D(this);

                Selection = AddDisposable(new SelectionSystem(this));

                OrderGenerator = AddDisposable(new OrderGeneratorSystem(this));

                Panel.ClientSizeChanged += OnPanelSizeChanged;
                OnPanelSizeChanged(this, EventArgs.Empty);

                GameSystems.ForEach(gs => gs.Initialize());

                SetCursor("Arrow");

                var playerTemplate = ContentManager.IniDataContext.PlayerTemplates.Find(t => t.Side == "Civilian");
                CivilianPlayer = Player.FromTemplate(playerTemplate, ContentManager);

                _developerModeView = AddDisposable(new DeveloperModeView(this));

                LauncherImage = LoadLauncherImage();

                _mapTimer.Reset();

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

        // Needed by Data Viewer.
        public void SetCursor(Cursor cursor)
        {
            _currentCursor = cursor;

            Panel.SetCursor(cursor);
        }

        public void SetCursor(string cursorName)
        {
            if (!_cachedCursors.TryGetValue(cursorName, out var cursor))
            {
                var mouseCursor = ContentManager.IniDataContext.MouseCursors.Find(x => x.Name == cursorName);
                if (mouseCursor == null)
                {
                    return;
                }

                var cursorFileName = mouseCursor.Image;
                if (string.IsNullOrEmpty(Path.GetExtension(cursorFileName)))
                {
                    cursorFileName += ".ani";
                }

                string cursorDirectory;
                switch (SageGame)
                {
                    case SageGame.Cnc3:
                    case SageGame.Cnc3KanesWrath:
                        // TODO: Get version number dynamically.
                        cursorDirectory = Path.Combine("RetailExe", "1.0", "Data", "Cursors");
                        break;

                    default:
                        cursorDirectory = Path.Combine("Data", "Cursors");
                        break;
                }

                var cursorFilePath = Path.Combine(_fileSystem.RootDirectory, cursorDirectory, cursorFileName);

                _cachedCursors[cursorName] = cursor = AddDisposable(new Cursor(cursorFilePath));
            }

            SetCursor(cursor);
        }

        public void ShowMainMenu()
        {
            var useShellMap = Configuration.LoadShellMap;
            if (useShellMap)
            {
                var shellMapName = ContentManager.IniDataContext.GameData.ShellMapName;
                var mainMenuScene = ContentManager.Load<Scene3D>(shellMapName);
                Scene3D = mainMenuScene;
                Scripting.Active = true;
            }

            Definition.MainMenu.AddToScene(ContentManager, Scene2D, useShellMap);
        }

        private void StartGame(
            string mapFileName,
            IConnection connection,
            PlayerSetting?[] playerSettings,
            int localPlayerIndex,
            bool isMultiPlayer)
        {
            // TODO: Loading screen.
            while (Scene2D.WndWindowManager.OpenWindowCount > 0)
            {
                Scene2D.WndWindowManager.PopWindow();
            }

            while (Scene2D.AptWindowManager.OpenWindowCount > 0)
            {
                Scene2D.AptWindowManager.PopWindow();
            }

            Scene3D?.Dispose();
            Scene3D = null;

            Scene3D = ContentManager.Load<Scene3D>(mapFileName);

            if (Scene3D == null)
            {
                throw new Exception($"Failed to load Scene3D \"{mapFileName}\"");
            }


            var mapCache = ContentManager.IniDataContext.MapCaches.Find(x => x.Name == mapFileName.ToLower());
            if (mapCache == null)
            {
                throw new Exception($"Failed to load MapCache \"{mapFileName}\"");
            }

            NetworkMessageBuffer = new NetworkMessageBuffer(this, connection);

            if (isMultiPlayer)
            {
                var players = new Player[playerSettings.Length + 1];

                var availablePositions = new List<int>(mapCache.NumPlayers);
                for (var a = 1; a <= mapCache.NumPlayers; a++)
                {
                    availablePositions.Add(a);
                }

                foreach (var playerSetting in playerSettings)
                {
                    if (playerSetting?.StartPosition != null)
                    {
                        int pos = (int) playerSetting?.StartPosition;
                        availablePositions.Remove(pos);
                    }
                }

                players[0] = CivilianPlayer;

                for (var i = 1; i <= playerSettings.Length; i++)
                {
                    PlayerSetting? playerSetting = playerSettings[i - 1];
                    if (playerSetting == null)
                    {
                        continue;
                    }

                    var playerTemplate = ContentManager.IniDataContext.PlayerTemplates.Find(t => t.Side == playerSetting?.Side);
                    players[i] = Player.FromTemplate(playerTemplate, ContentManager, playerSetting);
                    var startPos = playerSetting?.StartPosition;

                    // startPos seems to be -1 for random, and 0 for observer/civilian
                    if (startPos == null || startPos == -1 || startPos == 0)
                    {
                        startPos = availablePositions.Last();
                        availablePositions.Remove((int) startPos);
                    }

                    var playerStartPosition = Scene3D.Waypoints[$"Player_{startPos}_Start"].Position;
                    playerStartPosition.Z += Scene3D.Terrain.HeightMap.GetHeight(playerStartPosition.X, playerStartPosition.Y);

                    if (playerTemplate.StartingBuilding != null)
                    {
                        var startingBuilding = Scene3D.GameObjects.Add(ContentManager.IniDataContext.Objects.Find(x => x.Name == playerTemplate.StartingBuilding), players[i]);
                        startingBuilding.Transform.Translation = playerStartPosition;
                        startingBuilding.Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathUtility.ToRadians(startingBuilding.Definition.PlacementViewAngle));

                        var startingUnit0 = Scene3D.GameObjects.Add(ContentManager.IniDataContext.Objects.Find(x => x.Name == playerTemplate.StartingUnits[0].Unit), players[i]);
                        var startingUnit0Position = playerStartPosition;
                        startingUnit0Position += Vector3.Transform(Vector3.UnitX, startingBuilding.Transform.Rotation) * startingBuilding.Definition.Geometry.MajorRadius;
                        startingUnit0.Transform.Translation = startingUnit0Position;

                        players[i].SelectUnits(new [] { startingBuilding });
                    }
                }


                Scene3D.SetPlayers(players, players[localPlayerIndex]);
            }

            if (Definition.ControlBar != null)
            {
                Scene2D.ControlBar = Definition.ControlBar.Create(Scene3D.LocalPlayer.Side, ContentManager);
                Scene2D.ControlBar.AddToScene(Scene2D);
            }

            // Reset everything, and run the first update on the first frame.
            CurrentFrame = 0;
            _mapTimer.Reset();
            _nextLogicUpdate = TimeSpan.Zero;
            _nextScriptingUpdate = TimeSpan.Zero;
            CumulativeLogicUpdateError = TimeSpan.Zero;
        }

        public void StartCampaign(string side)
        {
            // TODO: Difficulty

            var campaign = ContentManager.IniDataContext.Campaigns.Single(x => x.Name == side);
            var firstMission = campaign.Missions.Single(x => x.Name == campaign.FirstMission);

            StartGame(
                firstMission.Map,
                new EchoConnection(),
                null,
                0,
                false);
        }

        public void StartMultiPlayerGame(
            string mapFileName,
            IConnection connection,
            PlayerSetting?[] playerSettings,
            int localPlayerIndex)
        {
            StartGame(
                mapFileName,
                connection,
                playerSettings,
                localPlayerIndex,
                true);
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
                // Scripting updates happen at 30Hz.
                _nextScriptingUpdate += TimeSpan.FromMilliseconds(ScriptingUpdateInterval);
            }
        }

        internal void LocalLogicTick(IEnumerable<InputMessage> messages)
        {
            _mapTimer.Update();
            MapTime = _mapTimer.CurrentGameTime;

            _renderTimer.Update();
            RenderTime = _renderTimer.CurrentGameTime;

            InputMessageBuffer.PumpEvents(messages);

            // How close are we to the next logic frame?
            var tickT = (float) (1.0 - TimeSpanUtility.Max(_nextLogicUpdate - MapTime.TotalTime, TimeSpan.Zero)
                                     .TotalMilliseconds / LogicUpdateInterval);

            // We pass RenderTime to Scene2D so that the UI remains responsive even when the game is paused.
            Scene2D.LocalLogicTick(RenderTime, Scene3D?.LocalPlayer);
            Scene3D?.LocalLogicTick(MapTime, tickT);
        }

        private void CheckGlobalHotkeys()
        {
            if (Window.CurrentInputSnapshot.KeyEvents.Any(x => x.Down && x.Key == Key.F9))
            {
                ToggleLogicRunning();
            }

            if (!IsLogicRunning && Window.CurrentInputSnapshot.KeyEvents.Any(x => x.Down && x.Key == Key.F10))
            {
                Step();
            }

            if (Window.CurrentInputSnapshot.KeyEvents.Any(x => x.Down && x.Key == Key.F11))
            {
                DeveloperModeEnabled = !DeveloperModeEnabled;
            }

            if (Window.CurrentInputSnapshot.KeyEvents.Any(x => x.Down && x.Key == Key.Enter && (x.Modifiers.HasFlag(ModifierKeys.Alt))))
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
            Scene3D?.LogicTick(frame);

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
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            base.Dispose(disposeManagedResources);

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
    }
}
