using LLGfx;
using OpenSage.Content;
using OpenSage.Graphics;
using OpenSage.Input;
using OpenSage.Mathematics;
using OpenSage.Physics;

namespace OpenSage
{
    /// <summary>
    /// Base class for all game components. Components are added to entities and provide 
    /// the main building blocks for everything in the game. Components are controlled by
    /// associated <see cref="GameSystem" />s.
    /// </summary>
    public abstract class EntityComponent
    {
        /// <summary>
        /// Gets the entity that contains this component.
        /// </summary>
        public Entity Entity { get; internal set; }

        /// <summary>
        /// Gets the graphics system.
        /// </summary>
        public GraphicsSystem Graphics => Game.Graphics;

        ///// <summary>
        ///// Gets the input system.
        ///// </summary>
        //public InputSystem Input => Game.Input;

        ///// <summary>
        ///// Gets the physics system.
        ///// </summary>
        //public PhysicsSystem Physics => Game.Physics;

        public GraphicsDevice GraphicsDevice => Game.GraphicsDevice;

        public Scene Scene => Entity?.Scene;

        /// <summary>
        /// Gets the current <see cref="Game"/>.
        /// </summary>
        public Game Game => Scene?.Game;

        public ContentManager ContentManager => Game?.ContentManager;

        public TransformComponent Transform => Entity?.Transform;

        /// <summary>
        /// Gets an axis-aligned bounding box that encloses this game component. The base implementation
        /// returns a zero-volume bounding box around the parent entity's world-space position. Components
        /// that have a visual representation override this to return a more suitable bounding box.
        /// </summary>
        public virtual BoundingBox BoundingBox => new BoundingBox(Transform.WorldPosition, Transform.WorldPosition);

        internal void Initialize()
        {
            Start();
        }

        /// <summary>
        /// Called when the component is made active in the scene.
        /// If a component is added to the scene later, <see cref="Start"/>
        /// will be called at that time.
        /// </summary>
        protected virtual void Start()
        {

        }

        /// <summary>
        /// Called when the component is removed from the scene.
        /// </summary>
        internal void Uninitialize()
        {
            Destroy();
        }

        /// <summary>
        /// Called when the component is removed from the scene, or the scene is unloaded.
        /// </summary>
        protected virtual void Destroy()
        {

        }
    }
}
