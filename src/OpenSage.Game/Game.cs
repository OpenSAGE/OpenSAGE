using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Graphics;
using OpenSage.Graphics.Rendering;
using OpenSage.Gui.Wnd;
using OpenSage.Input;
using OpenSage.Logic;
using OpenSage.Scripting;
using Veldrid;

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

        public ContentManager ContentManager { get; private set; }

        public GraphicsDevice GraphicsDevice { get; }

        internal List<GameSystem> GameSystems { get; }

        /// <summary>
        /// Gets the graphics system.
        /// </summary>
        public GraphicsSystem Graphics { get; }

        /// <summary>
        /// Gets the input system.
        /// </summary>
        public InputSystem Input { get; }

        /// <summary>
        /// Gets the scripting system.
        /// </summary>
        public ScriptingSystem Scripting { get; }

        // TODO: Remove this when we have other ways of testing colliders.
        /// <summary>
        /// Gets the debug entity picker system.
        /// </summary>
        public DebugEntityPickerSystem EntityPicker { get; }

        public int FrameCount { get; private set; }

        public GameTime UpdateTime { get; private set; }

        public bool IsActive { get; set; }

        public bool IsRunning { get; private set; }

        public SageGame SageGame { get; }

        public GameWindow Window { get; }

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

        public Game(
            FileSystem fileSystem,
            SageGame sageGame,
            Type wndCallbacksType,
            Func<GameWindow> createGameWindow)
        {
            Window = AddDisposable(createGameWindow());

#if DEBUG
            const bool debug = true;
#else
            const bool debug = false;
#endif

            GraphicsDevice = AddDisposable(GraphicsDevice.CreateD3D11(
                new GraphicsDeviceOptions(debug, PixelFormat.D32_Float_S8_UInt, true),
                Window.NativeWindowHandle,
                (uint) Window.ClientBounds.Width,
                (uint) Window.ClientBounds.Height));

            SageGame = sageGame;

            _fileSystem = fileSystem;

            _gameTimer = AddDisposable(new GameTimer());
            _gameTimer.Start();

            _cachedCursors = new Dictionary<string, Cursor>();

            _wndCallbackResolver = new WndCallbackResolver(wndCallbacksType);

            ResetElapsedTime();

            ContentManager = AddDisposable(new ContentManager(
                this,
                _fileSystem, 
                GraphicsDevice,
                sageGame,
                _wndCallbackResolver));
            
            switch (sageGame)
            {
                case SageGame.Ra3:
                case SageGame.Ra3Uprising:
                case SageGame.Cnc4:
                    break;

                default:
                    ContentManager.IniDataContext.LoadIniFile(@"Data\INI\GameData.ini");
                    ContentManager.IniDataContext.LoadIniFile(@"Data\INI\Mouse.ini");
                    break;
            }

            switch (sageGame)
            {
                case SageGame.CncGenerals:
                case SageGame.CncGeneralsZeroHour:
                case SageGame.BattleForMiddleEarth:
                case SageGame.BattleForMiddleEarthII:
                    ContentManager.IniDataContext.LoadIniFile(@"Data\INI\ParticleSystem.ini");
                    break;
            }

            GameSystems = new List<GameSystem>();

            Input = AddDisposable(new InputSystem(this));

            Graphics = AddDisposable(new GraphicsSystem(this));

            Scripting = AddDisposable(new ScriptingSystem(this));

            EntityPicker = AddDisposable(new DebugEntityPickerSystem(this));

            Scene2D = new Scene2D(this);

            Window.ClientSizeChanged += OnWindowClientSizeChanged;
            OnWindowClientSizeChanged(this, EventArgs.Empty);

            GameSystems.ForEach(gs => gs.Initialize());

            SetCursor("Arrow");

            IsRunning = true;
        }

        private void OnWindowClientSizeChanged(object sender, EventArgs e)
        {
            var newSize = Window.ClientBounds.Size;

            GraphicsDevice.ResizeMainWindow(
                (uint) newSize.Width,
                (uint) newSize.Height);

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

            Window.SetCursor(cursor);
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

                _cachedCursors[cursorName] = cursor = AddDisposable(Platform.CurrentPlatform.CreateCursor(cursorFilePath));
            }

            SetCursor(cursor);
        }

        public void Tick()
        {
            if (!IsRunning)
            {
                return;
            }

            if (!Window.PumpEvents())
            {
                IsRunning = false;
                return;
            }

            _gameTimer.Update();

            var gameTime = _gameTimer.CurrentGameTime;

            UpdateTime = gameTime;

            Update(gameTime);
            Draw(gameTime);

            FrameCount += 1;
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            GraphicsDevice.WaitForIdle();

            base.Dispose(disposeManagedResources);
        }

        private void Update(GameTime gameTime)
        {
            Updating?.Invoke(this, new GameUpdatingEventArgs(gameTime));

            foreach (var gameSystem in GameSystems)
            {
                gameSystem.Update(gameTime);
            }

            Scene2D.Update(gameTime);

            Scene3D?.Update(gameTime);
        }

        private void Draw(GameTime gameTime)
        {
            foreach (var gameSystem in GameSystems)
            {
                gameSystem.Draw(gameTime);
            }
        }
    }
}
