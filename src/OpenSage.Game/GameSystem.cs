using System;
using System.Collections;
using System.Collections.Generic;
using OpenSage.Graphics.Rendering;

namespace OpenSage
{
    public abstract class GameSystem : DisposableBase
    {
        private readonly Dictionary<Type, IList> _componentLists;

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
            _componentLists = new Dictionary<Type, IList>();

            game.GameSystems.Add(this);
        }

        /// <summary>
        /// Registers a game system to watch for added or removed components of the specified type.
        /// </summary>
        protected void RegisterComponentList<TEntityComponent>(List<TEntityComponent> componentList)
            where TEntityComponent : EntityComponent
        {
            _componentLists.Add(typeof(TEntityComponent), componentList);
        }

        internal virtual void OnEntityComponentAdded(EntityComponent component)
        {
            var componentType = component.GetType();

            foreach (var componentList in FindComponentLists(componentType))
                componentList.Add(component);
        }

        internal virtual void OnEntityComponentRemoved(EntityComponent component)
        {
            var componentType = component.GetType();

            foreach (var componentList in FindComponentLists(componentType))
                componentList.Remove(component);
        }

        internal virtual void OnSceneChanging()
        {
            foreach (var componentList in _componentLists.Values)
                componentList.Clear();
        }

        internal virtual void OnSceneChanged() { }

        internal virtual void OnSwapChainChanged() { }

        private List<IList> FindComponentLists(Type componentType)
        {
            var result = new List<IList>();
            while (componentType != null)
            {
                if (_componentLists.TryGetValue(componentType, out IList componentList))
                    result.Add(componentList);
                foreach (var interfaceType in componentType.GetInterfaces())
                    if (_componentLists.TryGetValue(interfaceType, out componentList))
                        result.Add(componentList);
                componentType = componentType.BaseType;
            }
            return result;
        }

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
