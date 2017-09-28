using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSage
{
    /// <summary>
    /// Entities are the building blocks of scenes. A scene contains a hierarchy of entities.
    /// Each entity contains at least a <see cref="TransformComponent"/>, and may contain multiple
    /// other components.
    /// </summary>
    public sealed class Entity
    {
        public string Name { get; set; }

        private Scene _scene;

        internal Scene SceneDirect
        {
            get { return _scene; }
            set { _scene = value; }
        }

        public Scene Scene
        {
            get { return FindRoot()._scene; }
            set
            {
                if (GetParent() != null)
                {
                    throw new InvalidOperationException();
                }

                var oldScene = _scene;
                if (oldScene == value)
                {
                    return;
                }

                oldScene?.Entities.Remove(this);
                value?.Entities.Add(this);
            }
        }

        public EntityComponentCollection Components { get; }

        public TransformComponent Transform { get; }

        public Entity()
        {
            Components = new EntityComponentCollection(this);
            Components.Add(Transform = new TransformComponent());
        }

        public Entity GetParent()
        {
            return Transform.Parent?.Entity;
        }

        public IEnumerable<Entity> GetChildren()
        {
            return Transform.Children.Select(x => x.Entity);
        }

        public Entity FindRoot()
        {
            var root = this;
            Entity parent;
            while ((parent = root.GetParent()) != null)
            {
                root = parent;
            }
            return root;
        }

        public void AddChild(Entity child)
        {
            Transform.Children.Add(child.Transform);
        }
    }
}
