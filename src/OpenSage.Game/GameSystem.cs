using OpenSage.Graphics.Rendering;

namespace OpenSage
{
    public abstract class GameSystem : DisposableBase
    {
        /// <summary>
        /// Gets the current game.
        /// </summary>
        protected Game Game { get; }

        /// <summary>
        /// Creates a new <see cref="GameSystem"/>.
        /// </summary>
        protected GameSystem(Game game)
        {
            Game = game;

            game.GameSystems.Add(this);
        }

        internal virtual void OnSceneChanging() { }
        internal virtual void OnSceneChanged() { }

        /// <summary>
        /// Override this to perform any required setup.
        /// </summary>
        public virtual void Initialize()
        {

        }

        /// <summary>
        /// Override this method to process game logic.
        /// </summary>
        public virtual void Update(GameTime gameTime)
        {
        }

        /// <summary>
        /// Override this method with rendering code.
        /// </summary>
        public virtual void Draw(GameTime gameTime)
        {
        }

        internal virtual void BuildRenderList(RenderList renderList) { }
    }
}
