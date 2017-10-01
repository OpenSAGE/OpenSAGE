using System.Collections.Generic;
using LLGfx;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Graphics;
using OpenSage.Graphics.Animation;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Input;
using OpenSage.Logic.Object;
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
        /// Gets the input system.
        /// </summary>
        public InputSystem Input { get; }

        public Game(GraphicsDevice graphicsDevice, FileSystem fileSystem)
        {
            GraphicsDevice = graphicsDevice;

            _fileSystem = fileSystem;

            _gameTimer = AddDisposable(new GameTimer());
            _gameTimer.Start();

            ContentManager = AddDisposable(new ContentManager(_fileSystem, graphicsDevice));

            GameSystems = new List<GameSystem>();

            AddDisposable(new AnimationSystem(this));
            AddDisposable(new ObjectSystem(this));
            AddDisposable(new ParticleSystemSystem(this));
            AddDisposable(new UpdateableSystem(this));

            Graphics = AddDisposable(new GraphicsSystem(this));

            Input = AddDisposable(new InputSystem(this));

            GameSystems.ForEach(gs => gs.Initialize());
        }

        public void SetSwapChain(SwapChain swapChain)
        {
            SwapChain = swapChain;
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
                    AddComponentsRecursive(_scene.Entities);
                }
            }
        }

        private void RemoveComponentsRecursive(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                OnEntityComponentsRemoved(entity.Components);

                RemoveComponentsRecursive(entity.GetChildren());
            }
        }

        private void AddComponentsRecursive(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                OnEntityComponentsAdded(entity.Components);

                AddComponentsRecursive(entity.GetChildren());
            }
        }

        public void Tick()
        {
            _gameTimer.Update();

            var gameTime = _gameTimer.CurrentGameTime;

            Update(gameTime);
            Draw(gameTime);
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
