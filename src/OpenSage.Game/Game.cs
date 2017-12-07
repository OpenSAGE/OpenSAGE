using System.Collections.Generic;
using LL.Graphics3D;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Graphics;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Gui;
using OpenSage.Input;
using OpenSage.Logic.Object;
using OpenSage.Scripting;
using OpenSage.Settings;

namespace OpenSage
{
    public sealed class Game : DisposableBase
    {
        private readonly FileSystem _fileSystem;
        private readonly GameTimer _gameTimer;

        private Scene _scene;

        public GameSettings Settings { get; } = new GameSettings();

        public ContentManager ContentManager { get; private set; }

        public GraphicsDevice GraphicsDevice { get; }
        internal SwapChain SwapChain { get; private set; }

        internal List<GameSystem> GameSystems { get; }

        /// <summary>
        /// Gets the graphics system.
        /// </summary>
        public GraphicsSystem Graphics { get; }

        /// <summary>
        /// Gets the GUI system.
        /// </summary>
        public GuiSystem Gui { get; }

        /// <summary>
        /// Gets the input system.
        /// </summary>
        public InputSystem Input { get; }

        /// <summary>
        /// Gets the scripting system.
        /// </summary>
        public ScriptingSystem Scripting { get; }

        public int FrameCount { get; private set; }

        public GameTime UpdateTime { get; private set; }

        public bool IsActive { get; set; }

        public Game(GraphicsDevice graphicsDevice, FileSystem fileSystem)
        {
            GraphicsDevice = graphicsDevice;

            _fileSystem = fileSystem;

            _gameTimer = AddDisposable(new GameTimer());
            _gameTimer.Start();

            ContentManager = AddDisposable(new ContentManager(
                _fileSystem, 
                graphicsDevice));

            ContentManager.IniDataContext.LoadIniFile(@"Data\INI\GameData.ini");

            GameSystems = new List<GameSystem>();

            AddDisposable(new AnimationSystem(this));
            AddDisposable(new ObjectSystem(this));
            AddDisposable(new ParticleSystemSystem(this));
            AddDisposable(new UpdateableSystem(this));

            Graphics = AddDisposable(new GraphicsSystem(this));

            Input = AddDisposable(new InputSystem(this));

            Scripting = AddDisposable(new ScriptingSystem(this));

            Gui = AddDisposable(new GuiSystem(this));

            GameSystems.ForEach(gs => gs.Initialize());
        }

        public void SetSwapChain(SwapChain swapChain)
        {
            SwapChain = swapChain;
            if (Scene != null)
            {
                Scene.Camera.SetSwapChain(swapChain);
            }
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
                    RemoveComponentsRecursive(oldScene.Entities);
                    oldScene.Game = null;
                }

                _scene = value;

                if (_scene != null)
                {
                    _scene.Game = this;

                    _scene.Camera.SetSwapChain(SwapChain);

                    AddComponentsRecursive(_scene.Entities);
                }
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

        public void Tick()
        {
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
