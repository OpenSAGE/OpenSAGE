using System.Collections.Generic;
using LLGfx;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Graphics;

namespace OpenSage
{
    public sealed class Game : DisposableBase
    {
        private readonly FileSystem _fileSystem;
        private readonly GameTimer _gameTimer;

        private Scene _scene;

        public ContentManager ContentManager { get; private set; }

        public GraphicsDevice GraphicsDevice { get; }
        internal SwapChain SwapChain { get; private set; }

        internal List<GameSystem> GameSystems { get; }

        /// <summary>
        /// Gets the graphics system.
        /// </summary>
        public GraphicsSystem Graphics { get; private set; }

        public Game(GraphicsDevice graphicsDevice, FileSystem fileSystem)
        {
            GraphicsDevice = graphicsDevice;

            _fileSystem = fileSystem;

            _gameTimer = AddDisposable(new GameTimer());
            _gameTimer.Start();

            ContentManager = AddDisposable(new ContentManager(_fileSystem, graphicsDevice));

            GameSystems = new List<GameSystem>();

            Graphics = AddDisposable(new GraphicsSystem(this));

            GameSystems.ForEach(gs => gs.Initialize());
        }

        public void Initialize(SwapChain swapChain)
        {
            SwapChain = swapChain;
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
