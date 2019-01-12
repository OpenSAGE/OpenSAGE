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
using OpenSage.Graphics.Rendering;
using OpenSage.Gui.Wnd;
using OpenSage.Input;
using OpenSage.Logic;
using OpenSage.Mathematics;
using OpenSage.Network;
using OpenSage.Scripting;
using Veldrid;
using Veldrid.ImageSharp;
using Player = OpenSage.Logic.Player;

namespace OpenSage
{
    public sealed class Game : DisposableBase
    {
        private readonly FileSystem _fileSystem;
        private readonly GameTimer _gameTimer;
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

        public int CurrentFrame { get; private set; }

        public GameTime UpdateTime { get; private set; }
        private TimeSpan _nextLogicUpdate;
        private TimeSpan _nextScriptingUpdate;

        public TimeSpan CumulativeLogicUpdateError;

        /// <summary>
        /// Is the game running?
        /// This is only false when the game is shutting down.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Is the game running logic updates?
        /// </summary>
        public bool IsLogicRunning { get; internal set; }

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
            GraphicsBackend? preferredBackend)
        {
            // TODO: Should we receive this as an argument? Do we need configuration in this constructor?
            Configuration = new Configuration();

            // TODO: Read game version from assembly metadata or .git folder
            // TODO: Set window icon.
            Window = AddDisposable(new GameWindow($"OpenSAGE - {installation.Game.DisplayName} - master", 100, 100, 1024, 768, preferredBackend));
            GraphicsDevice = Window.GraphicsDevice;

            Panel = AddDisposable(new GamePanel(GraphicsDevice));

            InputMessageBuffer = new InputMessageBuffer();

            Definition = installation.Game;

            _fileSystem = AddDisposable(installation.CreateFileSystem());

            _gameTimer = AddDisposable(new GameTimer());
            _gameTimer.Start();

            _cachedCursors = new Dictionary<string, Cursor>();

            _wndCallbackResolver = new WndCallbackResolver();

            ResetElapsedTime();

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

            IsRunning = true;
            IsLogicRunning = true;
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

        public void ResetElapsedTime()
        {
            _gameTimer.Reset();
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
            if (Configuration.LoadShellMap)
            {
                var shellMapName = ContentManager.IniDataContext.GameData.ShellMapName;
                var mainMenuScene = ContentManager.Load<Scene3D>(shellMapName);
                Scene3D = mainMenuScene;
                Scripting.Active = true;
            }

            Definition.MainMenu.AddToScene(ContentManager, Scene2D);
        }

        public void StartGame(string mapFileName, IConnection connection, PlayerSetting[] playerSettings, int localPlayerIndex)
        {
            // TODO: Loading screen.
            while (Scene2D.WndWindowManager.OpenWindowCount > 0)
            {
                Scene2D.WndWindowManager.PopWindow();
            }

            Scene3D = ContentManager.Load<Scene3D>(mapFileName);

            if (Scene3D == null)
            {
                throw new Exception($"Failed to load Scene3D \"{mapFileName}\"");
            }

            NetworkMessageBuffer = new NetworkMessageBuffer(this, connection);

            var players = new Player[playerSettings.Length + 1];

            for (var i = 0; i < playerSettings.Length; i++)
            {
                var playerTemplate = ContentManager.IniDataContext.PlayerTemplates.Find(t => t.Side == playerSettings[i].Side);
                players[i] = Player.FromTemplate(playerTemplate, ContentManager, playerSettings[i]);

                var player1StartPosition = Scene3D.Waypoints[$"Player_{i + 1}_Start"].Position;
                player1StartPosition.Z += Scene3D.Terrain.HeightMap.GetHeight(player1StartPosition.X, player1StartPosition.Y);

                if (playerTemplate.StartingBuilding != null)
                {
                    var startingBuilding = Scene3D.GameObjects.Add(ContentManager.IniDataContext.Objects.Find(x => x.Name == playerTemplate.StartingBuilding), players[i]);
                    startingBuilding.Transform.Translation = player1StartPosition;
                    startingBuilding.Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathUtility.ToRadians(startingBuilding.Definition.PlacementViewAngle));

                    var startingUnit0 = Scene3D.GameObjects.Add(ContentManager.IniDataContext.Objects.Find(x => x.Name == playerTemplate.StartingUnit0), players[i]);
                    var startingUnit0Position = player1StartPosition;
                    startingUnit0Position += Vector3.Transform(Vector3.UnitX, startingBuilding.Transform.Rotation) * startingBuilding.Definition.Geometry.MajorRadius;
                    startingUnit0.Transform.Translation = startingUnit0Position;
                }
            }

            players[players.Length - 1] = CivilianPlayer;

            Scene3D.SetPlayers(players, players[localPlayerIndex]);

            if (Definition.ControlBar != null)
            {
                Scene2D.ControlBar = Definition.ControlBar.Create(playerSettings[localPlayerIndex].Side, ContentManager);
                Scene2D.ControlBar.AddToScene(Scene2D);
            }

            // Reset everything, and run the first update on the first frame.
            CurrentFrame = 0;
            _gameTimer.Reset();
            _nextLogicUpdate = TimeSpan.Zero;
            _nextScriptingUpdate = TimeSpan.Zero;
            CumulativeLogicUpdateError = TimeSpan.Zero;
        }

        public void EndGame()
        {
            // TODO
            Scene3D = null;
            NetworkMessageBuffer = null;
        }

        public void Run()
        {
            _nextLogicUpdate = UpdateTime.TotalGameTime;
            _nextScriptingUpdate = UpdateTime.TotalGameTime;

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
            EveryFrameLogicTick(messages);

            if (Window.CurrentInputSnapshot.KeyEvents.Any(x => x.Down && x.Key == Key.F11))
            {
                DeveloperModeEnabled = !DeveloperModeEnabled;
            }

            // If the game is not paused and it's time to do a logic update, do so.
            if (IsLogicRunning && UpdateTime.TotalGameTime >= _nextLogicUpdate)
            {
                LogicTick();
                CumulativeLogicUpdateError += (UpdateTime.TotalGameTime - _nextLogicUpdate);
                // Logic updates happen at 5Hz.
                _nextLogicUpdate = UpdateTime.TotalGameTime.Add(TimeSpan.FromMilliseconds(1000.0 / 5.0));
            }

            // TODO: Which update should be performed first?
            if (IsLogicRunning && UpdateTime.TotalGameTime >= _nextScriptingUpdate)
            {
                Scripting.ScriptingTick();
                // Scripting updates happen at 30Hz.
                _nextScriptingUpdate = UpdateTime.TotalGameTime.Add(TimeSpan.FromMilliseconds(1000.0 / 30.0));
            }
        }

        internal void EveryFrameLogicTick(IEnumerable<InputMessage> messages)
        {
            _gameTimer.Update();
            UpdateTime = _gameTimer.CurrentGameTime;

            InputMessageBuffer.PumpEvents(messages);
            Updating?.Invoke(this, new GameUpdatingEventArgs(UpdateTime));
        }

        internal void Render()
        {
            Scene2D.Update(UpdateTime, Scene3D?.LocalPlayer);
            Scene3D?.Update(UpdateTime);

            Graphics.Draw(UpdateTime);
        }

        internal void LogicTick()
        {
            NetworkMessageBuffer?.Tick();

            foreach (var gameSystem in GameSystems)
            {
                gameSystem.LogicTick(CurrentFrame);
            }

            CurrentFrame += 1;
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
