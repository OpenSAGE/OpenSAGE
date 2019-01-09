﻿using System;
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

using Player = OpenSage.Logic.Player;

namespace OpenSage
{
    public sealed class Game : DisposableBase
    {
        public event EventHandler<GameUpdatingEventArgs> Updating;
        public event EventHandler<Rendering2DEventArgs> Rendering2D;
        public event EventHandler<BuildingRenderListEventArgs> BuildingRenderList;

        internal void RaiseRendering2D(Rendering2DEventArgs args)
        {
            Rendering2D?.Invoke(this, args);
        }

        internal void RaiseBuildingRenderList(BuildingRenderListEventArgs args)
        {
            BuildingRenderList?.Invoke(this, args);
        }

        private readonly FileSystem _fileSystem;
        private readonly GameTimer _gameTimer;
        private readonly WndCallbackResolver _wndCallbackResolver;

        private readonly Dictionary<string, Cursor> _cachedCursors;
        private Cursor _currentCursor;

        private readonly DeveloperModeView _developerModeView;

        private readonly TextureCopier _textureCopier;

        public ContentManager ContentManager { get; private set; }

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

        public int FrameCount { get; private set; }

        public GameTime UpdateTime { get; private set; }

        public bool IsActive { get; set; }

        public bool IsRunning { get; private set; }

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

        public Game(
            GameInstallation installation,
            GraphicsBackend? preferredBackend)
        {
            // TODO: Should we receive this as an argument? Do we need configuration in this constructor?
            Configuration = new Configuration();

            // TODO: Read game version from assembly metadata or .git folder
            // TODO: Set window icon.
            Window = AddDisposable(new GameWindow("OpenSAGE (master)", 100, 100, 1024, 768, preferredBackend));
            GraphicsDevice = Window.GraphicsDevice;

            Panel = AddDisposable(new GamePanel(GraphicsDevice));

            InputMessageBuffer = AddDisposable(new InputMessageBuffer());

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

            IsRunning = true;
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
        }

        public void EndGame()
        {
            // TODO
            Scene3D = null;
            NetworkMessageBuffer = null;
        }

        public void Run()
        {
            while (IsRunning)
            {
                if (!Window.PumpEvents())
                {
                    break;
                }

                if (Window.CurrentInputSnapshot.KeyEvents.Any(x => x.Down && x.Key == Key.F11))
                {
                    DeveloperModeEnabled = !DeveloperModeEnabled;
                }

                if (DeveloperModeEnabled)
                {
                    _developerModeView.Tick();
                }
                else
                {
                    Panel.EnsureFrame(Window.ClientBounds);

                    Tick(Window.MessageQueue);

                    _textureCopier.Execute(
                        Panel.Framebuffer.ColorTargets[0].Target,
                        GraphicsDevice.SwapchainFramebuffer);
                }

                Window.MessageQueue.Clear();

                GraphicsDevice.SwapBuffers();
            }

            // TODO: Cleanup resources.
        }

        internal void Tick(IEnumerable<InputMessage> inputMessages)
        {
            if (!IsRunning)
            {
                return;
            }

            _gameTimer.Update();

            var gameTime = _gameTimer.CurrentGameTime;

            UpdateTime = gameTime;

            Update(inputMessages, gameTime);

            Graphics.Draw(gameTime);

            FrameCount += 1;
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            base.Dispose(disposeManagedResources);

            GC.Collect();
        }

        private void Update(IEnumerable<InputMessage> inputMessages, GameTime gameTime)
        {
            InputMessageBuffer.PumpEvents(inputMessages);

            NetworkMessageBuffer?.Tick();

            Updating?.Invoke(this, new GameUpdatingEventArgs(gameTime));

            foreach (var gameSystem in GameSystems)
            {
                gameSystem.Update(gameTime);
            }

            Scene2D.Update(gameTime, Scene3D?.LocalPlayer);

            Scene3D?.Update(gameTime);
        }
    }
}
