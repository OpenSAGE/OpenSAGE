using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Graphics;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Gui.Apt;
using OpenSage.Gui.Wnd;
using OpenSage.Input;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Scripting;
using Veldrid;

namespace OpenSage
{
    public sealed class Game : DisposableBase
    {
        private readonly FileSystem _fileSystem;
        private readonly GameTimer _gameTimer;
        private readonly WndCallbackResolver _wndCallbackResolver;

        private readonly Dictionary<string, Cursor> _cachedCursors;
        private Cursor _currentCursor;

        private Scene _scene;

        public ContentManager ContentManager { get; private set; }

        public GraphicsDevice GraphicsDevice { get; }

        internal List<GameSystem> GameSystems { get; }

        /// <summary>
        /// Gets the graphics system.
        /// </summary>
        public GraphicsSystem Graphics { get; }

        /// <summary>
        /// Gets the GUI system.
        /// </summary>
        public AptSystem Apt { get; }

        /// <summary>
        /// Gets the GUI system.
        /// </summary>
        public ShapeSystem Shape { get; }

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

            Window.ClientSizeChanged += (sender, e) =>
            {
                GraphicsDevice.ResizeMainWindow(
                    (uint) Window.ClientBounds.Width,
                    (uint) Window.ClientBounds.Height);

                if (Scene != null)
                {
                    Scene.Camera.OnWindowSizeChanged(Window);
                    Scene.Scene2D.WndWindowManager.OnViewportSizeChanged();
                }

                foreach (var gameSystem in GameSystems)
                {
                    gameSystem.OnSwapChainChanged();
                }
            };

            ResetElapsedTime();

            ContentManager = AddDisposable(new ContentManager(
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

            GameSystems = new List<GameSystem>();

            Input = AddDisposable(new InputSystem(this));

            AddDisposable(new AnimationSystem(this));
            AddDisposable(new ObjectSystem(this));
            AddDisposable(new ParticleSystemSystem(this));

            Apt = AddDisposable(new AptSystem(this));

            Shape = AddDisposable(new ShapeSystem(this));

            Graphics = AddDisposable(new GraphicsSystem(this));

            Scripting = AddDisposable(new ScriptingSystem(this));

            EntityPicker = AddDisposable(new DebugEntityPickerSystem(this));

            GameSystems.ForEach(gs => gs.Initialize());

            SetCursor("Arrow");

            IsRunning = true;
        }

        public void ResetElapsedTime()
        {
            _gameTimer.Reset();
        }

        public void Activate()
        {
            // TODO: Unpause everything.
        }

        public void Deactivate()
        {
            // TODO: Pause everything.
        }

        public Scene Scene
        {
            get { return _scene; }
            set
            {
                var oldScene = _scene;
                if (oldScene == value)
                {
                    return;
                }

                if (oldScene != null)
                {
                    foreach (var system in GameSystems)
                        system.OnSceneChanging();

                    RemoveComponentsRecursive(oldScene.Entities);
                    oldScene.Game = null;
                }

                _scene = value;

                if (_scene != null)
                {
                    _scene.Game = this;

                    _scene.Scene2D = new Scene2D(this);

                    _scene.Camera.OnWindowSizeChanged(Window);

                    if (_scene.CameraController == null)
                    {
                        _scene.CameraController = new RtsCameraController(ContentManager);
                    }

                    AddComponentsRecursive(_scene.Entities);
                }

                foreach (var system in GameSystems)
                    system.OnSceneChanged();
            }
        }

        private void RemoveComponentsRecursive(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                RemoveComponentsRecursive(entity);
            }
        }

        internal void RemoveComponentsRecursive(Entity entity)
        {
            OnEntityComponentsRemoved(entity.Components);

            RemoveComponentsRecursive(entity.GetChildren());
        }

        private void AddComponentsRecursive(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                AddComponentsRecursive(entity);
            }
        }

        internal void AddComponentsRecursive(Entity entity)
        {
            OnEntityComponentsAdded(entity.Components);

            AddComponentsRecursive(entity.GetChildren());
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

        private void Update(GameTime gameTime)
        {
            foreach (var gameSystem in GameSystems)
                gameSystem.Update(gameTime);

            Scene?.Update(gameTime);
        }

        private void Draw(GameTime gameTime)
        {
            foreach (var gameSystem in GameSystems)
                gameSystem.Draw(gameTime);
        }

        internal void OnEntityComponentsAdded(IEnumerable<EntityComponent> components)
        {
            foreach (var component in components)
            {
                component.Initialize();

                foreach (var system in GameSystems)
                    system.OnEntityComponentAdded(component);
            }
        }

        internal void OnEntityComponentsRemoved(IEnumerable<EntityComponent> components)
        {
            foreach (var component in components)
            {
                component.Uninitialize();

                foreach (var system in GameSystems)
                    system.OnEntityComponentRemoved(component);
            }
        }
    }
}
